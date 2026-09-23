"""Render a Markdown document under docs/ to PDF (RFC 0008).

    python scripts/docs-pdf.py SOURCE.md OUT.pdf [--base DIR] [--lang en|ja] [--source-note TEXT]

SOURCE.md is the English document, or for the Japanese PDF a temporary Japanese
translation of it that keeps the English relative links. --base is the folder
relative links and images resolve against; it defaults to SOURCE.md's folder, so
pass the English document's folder when rendering a translation kept elsewhere.
--source-note goes in the page footer, e.g. "001_BD version 8 (2026-09-23)".

Needs Python's `markdown` package (pip install markdown) and Google Chrome.
Chrome is found through the CHROME environment variable, then the usual install
paths. ```mermaid blocks are drawn by Mermaid, loaded from cdn.jsdelivr.net at a
pinned version, so rendering a document with diagrams needs network access.
"""
import argparse, html, os, pathlib, re, shutil, subprocess, sys, tempfile

import markdown

MERMAID = "https://cdn.jsdelivr.net/npm/mermaid@11.17.2/dist/mermaid.esm.min.mjs"
CHROME_PATHS = [
    r"C:\Program Files\Google\Chrome\Application\chrome.exe",
    r"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
    "google-chrome", "google-chrome-stable", "chromium",
]

CSS = """
@page { size: A4 landscape; margin: 14mm 12mm 16mm;
  @bottom-left { content: "__NOTE__"; font: 8pt __FONT__; color: #6b7280; }
  @bottom-right { content: counter(page) " / " counter(pages); font: 8pt __FONT__; color: #6b7280; } }
body { font-family: __FONT__; font-size: 10.5pt; line-height: 1.55; color: #1f2328; }
h1 { font-size: 20pt; border-bottom: 1px solid #d0d7de; padding-bottom: 6px; margin: 0 0 14px; }
h2 { font-size: 15pt; border-bottom: 1px solid #d0d7de; padding-bottom: 4px; margin: 22px 0 10px; break-after: avoid; }
h3 { font-size: 13pt; margin: 18px 0 8px; break-after: avoid; }
h4 { font-size: 11.5pt; margin: 14px 0 6px; break-after: avoid; }
h5 { font-size: 10.5pt; margin: 12px 0 4px; break-after: avoid; }
p, ul, ol { margin: 0 0 9px; }
table { border-collapse: collapse; width: 100%; margin: 0 0 12px; font-size: 8.8pt; line-height: 1.4; }
thead { display: table-header-group; }
tr, img, pre, .mermaid { break-inside: avoid; }
th, td { border: 1px solid #d0d7de; padding: 4px 6px; text-align: left; vertical-align: top; }
th { background: #f6f8fa; font-weight: 600; }
tr:nth-child(2n) td { background: #fbfcfd; }
code { font-family: Consolas, "BIZ UDGothic", monospace; font-size: 90%; background: #eff1f3; border-radius: 3px; padding: 1px 4px; }
pre { background: #f6f8fa; border: 1px solid #d0d7de; border-radius: 4px; padding: 10px 12px; margin: 0 0 12px; }
pre code { background: none; padding: 0; font-size: 9pt; }
pre.mermaid { background: none; border: none; text-align: center; margin: 4px 0 12px; }
/* A figure never runs past a page: the content box is about 180mm high (A4 landscape less margins). */
pre.mermaid svg { max-width: 100%; max-height: 160mm; height: auto; }
img { max-width: 100%; max-height: 160mm; display: block; margin: 6px 0 12px; }
/* The line that introduces a figure ("Screen transition:") stays on the figure's page. */
p:has(+ pre.mermaid), p:has(+ p > img), h2 + p:has(> img), p:has(> img) { break-after: avoid; break-inside: avoid; }
"""
FONTS = {
    "en": '"Segoe UI", Helvetica, Arial, sans-serif',
    "ja": '"Yu Gothic UI", "Yu Gothic", Meiryo, "Hiragino Sans", "Noto Sans JP", sans-serif',
}


def find_chrome():
    for c in [os.environ.get("CHROME")] + CHROME_PATHS:
        if c and (os.path.exists(c) or shutil.which(c)):
            return c
    sys.exit("Chrome not found; set CHROME to its path.")


def build_html(text, base, lang, note):
    text = re.sub(r"<!--.*?-->", "", text, flags=re.S)  # template/source comments
    body = markdown.markdown(text, extensions=["tables", "fenced_code", "sane_lists"])
    # ```mermaid → <pre class="mermaid">; the text stays HTML-escaped, Mermaid reads textContent.
    body = re.sub(r'<pre><code class="language-mermaid">(.*?)</code></pre>',
                  r'<pre class="mermaid">\1</pre>', body, flags=re.S)
    # Keep short one-word cells (IDs, dates) on one line.
    body = re.sub(r"<td>([^<\s]{1,20})</td>", lambda m: f'<td style="white-space:nowrap">{m.group(1)}</td>', body)
    font = FONTS[lang]
    css = CSS.replace("__FONT__", font).replace("__NOTE__", note.replace('"', "'"))
    title = re.search(r"^# (.+)$", text, flags=re.M)
    has_mermaid = 'class="mermaid"' in body
    # Mermaid measures labels in the page's own font (after it has loaded), so the drawn text fits the
    # boxes it laid out; the extra node and rank spacing keeps edge labels off the nodes.
    config = ('{startOnLoad:false,theme:"default",themeVariables:{fontFamily:%s,fontSize:"13px"},'
              'flowchart:{nodeSpacing:70,rankSpacing:120,padding:10,wrappingWidth:200},'
              'state:{nodeSpacing:55,rankSpacing:60},er:{fontSize:12}}' % repr(font.replace('"', "'")))
    script = (f'<script type="module">import mermaid from "{MERMAID}";await document.fonts.ready;'
              f'mermaid.initialize({config});await mermaid.run({{querySelector:".mermaid"}});</script>'
              if has_mermaid else "")
    return (f'<!doctype html><html lang="{lang}"><head><meta charset="utf-8">'
            f'<base href="{pathlib.Path(base).resolve().as_uri()}/">'
            f'<title>{html.escape(title.group(1) if title else "")}</title><style>{css}</style></head>'
            f"<body>{body}{script}</body></html>")


def main():
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("source"); ap.add_argument("out")
    ap.add_argument("--base"); ap.add_argument("--lang", choices=FONTS, default="en")
    ap.add_argument("--source-note", default="")
    a = ap.parse_args()
    src = pathlib.Path(a.source)
    base = pathlib.Path(a.base) if a.base else src.parent
    page = build_html(src.read_text(encoding="utf-8"), base, a.lang, a.source_note)
    out = pathlib.Path(a.out).resolve()
    out.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory() as tmp:
        h, p = pathlib.Path(tmp, "doc.html"), pathlib.Path(tmp, "doc.pdf")
        h.write_text(page, encoding="utf-8")
        # Chrome writes into its temp folder, then the PDF is moved into place.
        subprocess.run([find_chrome(), "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
                        "--allow-file-access-from-files", "--virtual-time-budget=20000",
                        f"--print-to-pdf={p}", h.as_uri()],
                       check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        if not p.exists():
            sys.exit("Chrome produced no PDF.")
        shutil.move(str(p), out)
    print(out)


if __name__ == "__main__":
    main()
