using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ResumeApp.Data;
using ResumeApp.Models;
using System.IO;
using System.Security.Claims;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Linq;



namespace ResumeApp.Controllers
{
    [Authorize]
    public class ResumeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".txt" };
        private readonly ActivityLogger _logger;
        public ResumeController(ApplicationDbContext context, IWebHostEnvironment env, ActivityLogger logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        private bool IsAllowedFileType(string fileName) =>
            AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower());

        private (string Name, string Email, string Phone, string Skills, string Experience, int Years) ParseResume(string text)
        {
            // Split into clean lines
            var lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList();

            // DEBUG: log first 20 lines
            for (int i = 0; i < Math.Min(20, lines.Count); i++)
                Console.WriteLine($"Line {i}: {lines[i]}");

            // Extract Email
            string email = Regex.Match(text, @"[\w\.-]+@[\w\.-]+\.\w+").Value;
            if (string.IsNullOrWhiteSpace(email)) email = "Not found";

            // Match Indian mobile numbers first, otherwise fallback to generic formats
            string phone = Regex.Match(text, @"(\+91[-\s]?)?[6-9]\d{9}|\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}").Value;
            if (string.IsNullOrWhiteSpace(phone)) phone = "Not found";


            // --- NAME DETECTION ---
            string name = lines
                .Take(30) // check first 30 lines
                .FirstOrDefault(line =>
                    !line.Contains("@") &&
                    !Regex.IsMatch(line, @"\d") &&
                    !line.ToLower().Contains("resume") &&
                    !line.ToLower().Contains("curriculum") &&
                    line.Length >= 2 && line.Length <= 60
                ) ?? "Not found";

            // Special case: extract a name from the first line even if merged with other text
            if (name == "Not found" && lines.Count > 0)
            {
                var firstLine = lines[0].Trim();

                // If the whole line is just letters/spaces, take it as the name
                if (Regex.IsMatch(firstLine, @"^[A-Za-z\s]+$") && firstLine.Length <= 60)
                {
                    name = firstLine;
                }
                else
                {
                    // Otherwise, just take the first two words as a possible name
                    var words = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length >= 2)
                        name = string.Join(" ", words.Take(2));
                    else if (words.Length == 1)
                        name = words[0];
                }
            }



            // Check for "Name:" labels
            if (name == "Not found")
            {
                var labelLine = lines.FirstOrDefault(l => l.ToLower().StartsWith("name:"));
                if (!string.IsNullOrEmpty(labelLine))
                    name = labelLine.Replace("Name:", "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            // Fallback: line before email/phone
            if (name == "Not found")
            {
                int idx = lines.FindIndex(l => l.Contains(email) || l.Contains(phone));
                if (idx > 0)
                {
                    var possibleName = lines[idx - 1];
                    if (!string.IsNullOrWhiteSpace(possibleName))
                        name = possibleName.Trim();
                }
            }

            // Extract Skills
            string skills = "Not found";
            var skillMatch = Regex.Match(text, @"(?i)(Skills|Technical Skills|Key Skills)\s*[:\-•]?\s*(.+?)(?=(\n|$|\r|\r\n|Education|Summary|Experience|Projects))", RegexOptions.Singleline);
            if (skillMatch.Success)
                skills = skillMatch.Groups[2].Value.Trim();

            // Extract Experience section
            string experience = "Not found";
            var expMatch = Regex.Match(text, @"(?i)(Work Experience|Professional Experience|Experience)[\s\S]{0,800}");
            if (expMatch.Success)
                experience = expMatch.Value.Trim();

            // Extract numeric years from experience
            int yearsExtracted = 0;
            var yearsMatch = Regex.Match(experience, @"(\d+(\.\d+)?)\s*(\+)?\s*(yrs?|years?)", RegexOptions.IgnoreCase);
            if (yearsMatch.Success && double.TryParse(yearsMatch.Groups[1].Value, out double totalYears))
                yearsExtracted = (int)Math.Floor(totalYears);

            return (name, email, phone, skills, experience, yearsExtracted);
        }



        private bool IsPdfFile(IFormFile file)
        {
            try
            {
                using (var reader = new BinaryReader(file.OpenReadStream()))
                {
                    var bytes = reader.ReadBytes(5);
                    return bytes[0] == 0x25 && bytes[1] == 0x50 &&
                           bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D;
                }
            }
            catch
            {
                return false;
            }
        }

        private string ExtractTextFromDocx(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            using (var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(stream, false))
            {
                var body = wordDoc.MainDocumentPart?.Document?.Body;
                return body?.InnerText ?? string.Empty;
            }
        }
        private Task<Resume?> FindExistingResumeAsync(string fileHash, string? email)
        {
            return _context.Resumes.FirstOrDefaultAsync(r =>
                r.FileHash == fileHash ||
                (!string.IsNullOrWhiteSpace(email) && r.Email == email));
        }

        private async Task<bool> LinkResumeToRequirementAsync(int resumeId, int requirementId, string? userId)
        {
            var alreadyLinked = await _context.ResumeRequirementLinks
                .AnyAsync(l => l.RequirementId == requirementId && l.ResumeId == resumeId);

            if (alreadyLinked) return false;

            _context.ResumeRequirementLinks.Add(new ResumeRequirementLink
            {
                ResumeId = resumeId,
                RequirementId = requirementId,
                LinkedByUserId = userId,
                LinkedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return true;
        }


        [HttpGet]
        public IActionResult Upload(int? requirementId)
        {
            ViewBag.RequirementId = requirementId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int? requirementId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["UploadError"] = "No file selected.";
                return RedirectToAction("Upload", new { requirementId });
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
            {
                TempData["UploadError"] = $"Unsupported file type: {extension}";
                return RedirectToAction("Upload", new { requirementId });
            }

            // Validate PDF magic bytes
            if (extension == ".pdf" && !IsPdfFile(file))
            {
                TempData["UploadError"] = "Invalid PDF file.";
                return RedirectToAction("Upload", new { requirementId });
            }

            var fileHash = ComputeFileHash(file);
            string extractedText = "";

            // Extract Text
            if (extension == ".pdf")
            {
                using var stream = file.OpenReadStream();
                using var pdf = PdfDocument.Open(stream);
                foreach (var page in pdf.GetPages())
                    extractedText += string.Join(" ", page.GetWords().Select(w => w.Text)) + "\n";
            }
            else if (extension == ".docx")
                extractedText = ExtractTextFromDocx(file);
            else if (extension == ".txt")
                using (var reader = new StreamReader(file.OpenReadStream()))
                    extractedText = await reader.ReadToEndAsync();

            // Parse & check duplicates
            var parsed = ParseResume(extractedText);

            // Strong: file hash; Fallback: email
            var existing = await FindExistingResumeAsync(fileHash, parsed.Email);

            if (existing != null)
            {
                if (requirementId.HasValue)
                {
                    var uploaderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var linked = await LinkResumeToRequirementAsync(existing.Id, requirementId.Value, uploaderId);

                    TempData["UploadSuccess"] = linked
                        ? $"Existing profile ({existing.Name ?? existing.Email ?? "Profile"}) linked to this JD."
                        : "This profile is already linked to this JD.";

                    return RedirectToAction("SharedProfiles", "ClientRequirements", new { id = requirementId.Value });
                }

                TempData["UploadError"] = "Duplicate resume detected: same file/email already exists.";
                return RedirectToAction("List");
            }
            // Save file
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);
            var filePath = Path.Combine(uploads, file.FileName);
            using (var saveStream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(saveStream);

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var resume = new Resume
            {
                FileName = file.FileName,
                FilePath = filePath,
                ExtractedText = extractedText,
                UploadedAt = DateTime.Now,
                Name = parsed.Name,
                Email = parsed.Email,
                Phone = parsed.Phone,
                Skills = parsed.Skills,
                Experience = parsed.Experience,
                FileHash = fileHash,
                YearsOfExperience = parsed.Years,
                UserId = currentUserId
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            // ✅ LINK TO REQUIREMENT
            if (requirementId.HasValue)
            {
                var link = new ResumeRequirementLink
                {
                    ResumeId = resume.Id,
                    RequirementId = requirementId.Value,
                    LinkedByUserId = currentUserId,
                    LinkedAt = DateTime.Now
                };

                _context.ResumeRequirementLinks.Add(link);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Linked Resume ID {resume.Id} to Requirement {requirementId.Value}");

                // ✅ Redirect to Shared Profiles
                return RedirectToAction("SharedProfiles", "ClientRequirements", new { id = requirementId.Value });
            }

            // fallback if no JD
            return RedirectToAction("List");
        }


        [HttpPost]
        public async Task<IActionResult> UploadMultiple(List<IFormFile> files, int? requirementId = null)
        {
            if (requirementId.HasValue)
            {
                TempData["UploadError"] = "Multi-upload is disabled when uploading against a JD. Use single upload.";
                return RedirectToAction("Upload", new { requirementId });
            }
            if (files == null || files.Count == 0)
            {
                TempData["UploadError"] = "Please select at least one resume to upload.";
                return RedirectToAction("Upload");
            }

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            int skipped = 0, uploaded = 0;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (!AllowedExtensions.Contains(extension))
                    {
                        skipped++;
                        continue; // Skip unsupported file type
                    }

                    var fileHash = ComputeFileHash(file);
                    string extractedText = "";

                    if (extension == ".pdf")
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            using (var pdf = PdfDocument.Open(stream))
                            {
                                foreach (var page in pdf.GetPages())
                                {
                                    extractedText += string.Join(" ", page.GetWords().Select(w => w.Text))
                                                   + Environment.NewLine + Environment.NewLine;
                                }

                            }
                        }
                    }
                    else if (extension == ".docx")
                    {
                        extractedText = ExtractTextFromDocx(file);
                    }
                    else if (extension == ".txt")
                    {
                        using (var reader = new StreamReader(file.OpenReadStream()))
                        {
                            extractedText = await reader.ReadToEndAsync();
                        }
                    }

                    var parsed = ParseResume(extractedText);

                    var existing = await FindExistingResumeAsync(fileHash, parsed.Email);
                    if (existing != null)
                    {
                        skipped++;
                        continue;
                    }


                    var filePath = Path.Combine(uploads, file.FileName);
                    using (var saveStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(saveStream);
                    }

                    var resume = new Resume
                    {
                        FileName = file.FileName,
                        FilePath = filePath,
                        ExtractedText = extractedText,
                        UploadedAt = DateTime.Now,
                        Name = parsed.Name,
                        Email = parsed.Email,
                        Phone = parsed.Phone,
                        Skills = parsed.Skills,
                        Experience = parsed.Experience,
                        FileHash = fileHash,
                        YearsOfExperience = parsed.Years,
                        UserId = userId

                    };

                    _context.Resumes.Add(resume);
                    uploaded++;
                    await _logger.LogAsync("Upload", $"Uploaded resume: {file.FileName}");
                }
            }

            await _context.SaveChangesAsync();

            TempData["UploadSuccess"] = $"{uploaded} resume(s) uploaded. {skipped} duplicate(s) or unsupported file(s) skipped.";
            return RedirectToAction("List");
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resume resume)
        {
            if (!ModelState.IsValid)
            {
                return View(resume);
            }

            if (resume.UploadFile != null && resume.UploadFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var filePath = Path.Combine(uploads, resume.UploadFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await resume.UploadFile.CopyToAsync(stream);
                }

                string extractedText = "";
                using (var pdf = PdfDocument.Open(filePath))
                {
                    foreach (var page in pdf.GetPages())
                    {
                        extractedText += page.Text;
                    }
                }

                var parsed = ParseResume(extractedText);

                resume.Name = parsed.Name;
                resume.Email = parsed.Email;
                resume.Phone = parsed.Phone;
                resume.Skills = parsed.Skills;
                resume.Experience = parsed.Experience;
                resume.FileName = resume.UploadFile.FileName;
                resume.FilePath = filePath;
                resume.ExtractedText = extractedText;
                resume.UploadedAt = DateTime.Now;
            }

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List(
              string? search,
              int? minExperience,
              string? mandatorySkills,
              string? optionalSkills,
              int page = 1,
              int pageSize = 100)
        {
            var resumes = _context.Resumes.AsQueryable();

            // 1️⃣ Experience filter (HARD)
            if (minExperience.HasValue)
            {
                resumes = resumes.Where(r => r.YearsOfExperience >= minExperience.Value);
            }

            // 2️⃣ Mandatory skills (ALL must exist)
            var mandatory = SplitSkills(mandatorySkills)
                .Select(NormalizeSkill)
                .ToList();

            foreach (var skill in mandatory)
            {
                resumes = resumes.Where(r =>
                    EF.Functions.Like(r.Skills.ToLower(), $"%{skill}%"));
            }

            // 3️⃣ Optional skills (score only)
            var optional = SplitSkills(optionalSkills)
                .Select(NormalizeSkill)
                .ToList();

            var scoredQuery = resumes.Select(r => new
            {
                Resume = r,
                OptionalScore = optional.Count == 0 ? 0 :
                    optional.Count(s => r.Skills.ToLower().Contains(s))
            });

            // 4️⃣ Old simple text search (fallback / optional)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                scoredQuery = scoredQuery.Where(x =>
                    EF.Functions.Like(x.Resume.Name.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(x.Resume.Email.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(x.Resume.Phone, $"%{search}%") ||
                    EF.Functions.Like(x.Resume.Skills.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(x.Resume.Experience.ToLower(), $"%{search}%"));
            }

            // 5️⃣ Ordering (BEST ATS BEHAVIOR)
            var ordered = scoredQuery
                .OrderByDescending(x => x.OptionalScore)
                .ThenByDescending(x => x.Resume.YearsOfExperience)
                .ThenByDescending(x => x.Resume.UploadedAt);

            int totalCount = await ordered.CountAsync();

            var result = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Resume)
                .Include(r => r.User)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            // preserve filters in UI
            ViewBag.MinExperience = minExperience;
            ViewBag.MandatorySkills = mandatorySkills;
            ViewBag.OptionalSkills = optionalSkills;

            return View(result);
        }


        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> MyUploads(string? search)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var resumes = _context.Resumes
                .Where(r => r.UserId == userId); //  Filter by logged-in reviewer

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                resumes = resumes.Where(r =>
                  EF.Functions.Like(r.Name, $"%{search}%")  ||
                  EF.Functions.Like(r.Email, $"%{search}%") ||
                  EF.Functions.Like(r.Phone, $"%{search}%") ||
                  EF.Functions.Like(r.Skills, $"%{search}%")||
                  EF.Functions.Like(r.Experience, $"%{search}%"));
            }

            resumes = resumes.Include(r => r.User).OrderByDescending(r => r.UploadedAt);
            var result = await resumes.ToListAsync();
            return View("List", result); //Reuse the same view
        }


        [HttpPost]
        public IActionResult SubmitFinal(ResumeFormViewModel model)
        {
            //  Here you can update Resume record or save to a new table
            // Example:
            var resume = _context.Resumes.Find(model.ResumeId);
            if (resume != null)
            {
                resume.Name = $"{model.FirstName} {model.LastName}";
                resume.Email = model.Email;
                resume.Phone = model.Phone;
                resume.Skills = model.Skills;
                resume.Experience = model.Experience;
                _context.SaveChanges();
            }

            return RedirectToAction("List");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // allow Admins
        public async Task<IActionResult> Delete(int id)
        {
            var resume = await _context.Resumes.FindAsync(id);
            if (resume != null)
            {
                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
                await _logger.LogAsync("Delete", $"Admin deleted resume ID: {resume.Id}, FileName: {resume.FileName}");
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null)
                return NotFound();
            return View(resume);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resume updated)
        {
            if (id != updated.Id) return NotFound();
            if (!ModelState.IsValid) return View(updated);

            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null) return NotFound();

            resume.Name = updated.Name;
            resume.Email = updated.Email;
            resume.Phone = updated.Phone;
            resume.Skills = updated.Skills;
            resume.Experience = updated.Experience;
            resume.YearsOfExperience = updated.YearsOfExperience;

            await _context.SaveChangesAsync();
            await _logger.LogAsync("Edit", $"Edited resume ID: {resume.Id}, New Name: {resume.Name}");
            Console.WriteLine("Editing Resume: " + updated.Name);

            return RedirectToAction("List");
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id);
            if (resume == null) return NotFound();

            return View(resume);
        }
        private string ComputeFileHash(IFormFile file)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var stream = file.OpenReadStream();
            var hashBytes = sha256.ComputeHash(stream);
            return Convert.ToBase64String(hashBytes);
        }
        [HttpGet]
        public async Task<IActionResult> DownloadZip(string? search)
        {
            var resumes = _context.Resumes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                resumes = resumes.Where(r =>
                    EF.Functions.Like(r.Name.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(r.Email.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(r.Phone, $"%{search}%") ||
                    EF.Functions.Like(r.Skills.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(r.Experience.ToLower(), $"%{search}%"));
            }

            var filtered = await resumes.ToListAsync();

            using var memoryStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var r in filtered)
                {
                    if (System.IO.File.Exists(r.FilePath))
                    {
                        var entry = archive.CreateEntry(Path.GetFileName(r.FilePath));
                        using var entryStream = entry.Open();
                        using var fileStream = System.IO.File.OpenRead(r.FilePath);
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            await _logger.LogAsync("Download", $"Downloaded resumes as zip ({filtered.Count} files)");
            return File(memoryStream.ToArray(), "application/zip", "FilteredResumes.zip");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRemark(int id, string remark)
        {
            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null) return NotFound();
            resume.Remark = remark;
            await _context.SaveChangesAsync();
            await _logger.LogAsync("Remark", $"Updated remark for resume ID: {resume.Id}");
            return RedirectToAction("List");
        }
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? search)
        {
            var resumes = _context.Resumes.Include(r => r.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                resumes = resumes.Where(r =>
                    EF.Functions.Like(r.Name.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(r.Email.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(r.Phone, $"%{search}%") ||
                    EF.Functions.Like(r.Skills.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(r.Experience.ToLower(), $"%{search}%"));
            }

            var data = await resumes.OrderByDescending(r => r.UploadedAt).ToListAsync();

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Resumes");

            // Headers
            string[] headers = { "Name", "Email", "Phone", "Skills", "Experience", "Years", "Uploaded By", "Uploaded At" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
            }

            // Data rows
            int row = 2;
            foreach (var r in data)
            {
                worksheet.Cells[row, 1].Value = r.Name;
                worksheet.Cells[row, 2].Value = r.Email;
                worksheet.Cells[row, 3].Value = r.Phone;

                // Truncate Skills and Experience
                string shortSkills = string.IsNullOrEmpty(r.Skills) ? "" : r.Skills.Length > 30 ? r.Skills.Substring(0, 30) + "..." : r.Skills;
                string shortExp = string.IsNullOrEmpty(r.Experience) ? "" : r.Experience.Length > 30 ? r.Experience.Substring(0, 30) + "..." : r.Experience;

                worksheet.Cells[row, 4].Value = shortSkills;
                worksheet.Cells[row, 5].Value = shortExp;

                worksheet.Cells[row, 6].Value = r.YearsOfExperience;
                worksheet.Cells[row, 7].Value = r.User?.FullName ?? "N/A";
                worksheet.Cells[row, 8].Value = r.UploadedAt.ToString("g");
                row++;
            }

            // Border styling
            var dataRange = worksheet.Cells[1, 1, row - 1, headers.Length];
            dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            // Custom widths (no wrapping needed)
            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 25;
            worksheet.Column(5).Width = 30;
            worksheet.Column(6).Width = 10;
            worksheet.Column(7).Width = 20;
            worksheet.Column(8).Width = 18;

            var excelData = package.GetAsByteArray();
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Resumes.xlsx");
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Reviewer,Vendor,Team Lead")]
        public async Task<IActionResult> LinkToRequirement(int resumeId, int requirementId)
        {
            var resume = await _context.Resumes.FindAsync(resumeId);
            var requirement = await _context.ClientRequirements.FindAsync(requirementId);

            if (resume == null || requirement == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // prevent duplicates
            var exists = await _context.ResumeRequirementLinks
                .AnyAsync(x => x.ResumeId == resumeId && x.RequirementId == requirementId);
            if (exists)
                return Json(new { success = false, message = "Resume already linked to this requirement." });

            var link = new ResumeRequirementLink
            {
                ResumeId = resumeId,
                RequirementId = requirementId,
                LinkedByUserId = userId
            };

            _context.ResumeRequirementLinks.Add(link);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Linked successfully!" });
        }
        private static List<string> SplitSkills(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            return input
                .ToLower()
                .Split(new[] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Distinct()
                .ToList();
        }

        // Phase-1 simple normalization
        private static string NormalizeSkill(string skill)
        {
            return skill switch
            {
                "c sharp" => "c#",
                "csharp" => "c#",
                "ms sql" => "sql",
                "mssql" => "sql",
                _ => skill
            };
        }

    }
}