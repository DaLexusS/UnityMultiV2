const fs = require("fs");
const path = require("path");

const { chromium } = require("playwright");

const root = process.cwd();
const sourcePath = path.join(root, "SubmissionChecklist.md");
const outputDir = path.join(root, "output", "pdf");
const outputPath = path.join(outputDir, "UnityMultiV2_SubmissionChecklist.pdf");
const htmlPath = path.join(outputDir, "UnityMultiV2_SubmissionChecklist.html");

function escapeHtml(value) {
  return value
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
}

function convertInlineCode(value) {
  return escapeHtml(value).replace(/`([^`]+)`/g, "<code>$1</code>");
}

function markdownToHtml(markdown) {
  const lines = markdown.split(/\r?\n/);
  const parts = [];

  for (const line of lines) {
    if (line.startsWith("# ")) {
      parts.push(`<h1>${convertInlineCode(line.slice(2))}</h1>`);
    } else if (line.startsWith("## ")) {
      parts.push(`<h2>${convertInlineCode(line.slice(3))}</h2>`);
    } else if (line.trim().length === 0) {
      parts.push("");
    } else if (line.startsWith("Scripts:") || line.startsWith("Prefabs:") || line.startsWith("Prefab:")) {
      parts.push(`<p class="scripts">${convertInlineCode(line)}</p>`);
    } else if (line.endsWith(":")) {
      parts.push(`<h3>${convertInlineCode(line.slice(0, -1))}</h3>`);
    } else {
      parts.push(`<p>${convertInlineCode(line)}</p>`);
    }
  }

  return parts.join("\n");
}

async function main() {
  fs.mkdirSync(outputDir, { recursive: true });

  const markdown = fs.readFileSync(sourcePath, "utf8");
  const content = markdownToHtml(markdown);

  const html = `<!doctype html>
<html lang="he" dir="rtl">
<head>
  <meta charset="utf-8">
  <style>
    @page {
      size: A4;
      margin: 18mm 16mm;
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      color: #171717;
      font-family: Arial, "Noto Sans Hebrew", sans-serif;
      font-size: 12.5px;
      line-height: 1.45;
      direction: rtl;
    }

    h1 {
      margin: 0 0 14px;
      font-size: 22px;
      line-height: 1.25;
      text-align: center;
    }

    h2 {
      margin: 18px 0 8px;
      padding-bottom: 4px;
      border-bottom: 1px solid #999;
      font-size: 17px;
      break-after: avoid;
    }

    h3 {
      margin: 10px 0 3px;
      font-size: 13.5px;
      font-weight: 700;
      break-after: avoid;
    }

    p {
      margin: 0 0 5px;
    }

    .scripts {
      direction: ltr;
      text-align: left;
      color: #333;
      background: #f3f3f3;
      border: 1px solid #ddd;
      border-radius: 4px;
      padding: 3px 5px;
      font-size: 11.5px;
      break-inside: avoid;
    }

    code {
      direction: ltr;
      unicode-bidi: embed;
      font-family: Consolas, "Courier New", monospace;
      font-size: 0.92em;
      background: #eee;
      padding: 1px 3px;
      border-radius: 3px;
    }
  </style>
</head>
<body>
${content}
</body>
</html>`;

  fs.writeFileSync(htmlPath, html, "utf8");

  const executablePath = fs.existsSync("C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe")
    ? "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe"
    : "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

  const browser = await chromium.launch({ headless: true, executablePath });
  const page = await browser.newPage();
  await page.goto(`file:///${htmlPath.replace(/\\/g, "/")}`, { waitUntil: "load" });
  await page.pdf({
    path: outputPath,
    format: "A4",
    printBackground: true,
    margin: {
      top: "18mm",
      right: "16mm",
      bottom: "18mm",
      left: "16mm",
    },
  });
  await browser.close();

  console.log(outputPath);
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
