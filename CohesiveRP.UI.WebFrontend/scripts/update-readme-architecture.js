#!/usr/bin/env node

/**
 * update-readme-architecture.js
 *
 * Scans the project root and regenerates the '# Project folders architecture'
 * section in README.md with an up-to-date file/folder tree.
 *
 * Usage:
 *   node scripts/update-readme-architecture.js
 *   npm run update-readme
 */

import { readdirSync, statSync, readFileSync, writeFileSync } from "fs";
import { join, relative, resolve } from "path";

// ── Configuration ────────────────────────────────────────────────────────────

/** Folders / files to skip entirely (matched against the entry name). */
const IGNORE_NAMES = new Set([
  "node_modules",
  "dist",
  "obj",
  "public",
  ".git",
  ".vite",
  ".idea",
  ".vscode",
  "__pycache__",
]);

/** File extensions to skip (keeps the tree readable). */
const IGNORE_EXTENSIONS = new Set([
  ".lock",      // package-lock.json, yarn.lock, etc.
]);

/** Maximum depth to recurse (undefined = unlimited). */
const MAX_DEPTH = undefined;

/** README section heading to replace (without the leading #). */
const SECTION_HEADING = "# Project folders architecture";

// ── Tree builder ─────────────────────────────────────────────────────────────

/**
 * Returns true if the entry should be excluded from the tree.
 * @param {string} name  - bare file/folder name
 */
function isIgnored(name) {
  if (IGNORE_NAMES.has(name)) return true;
  if (name.startsWith(".") && name !== ".env.example") return true; // skip dot-files except whitelisted
  const lastDot = name.lastIndexOf(".");
  if (lastDot !== -1 && IGNORE_EXTENSIONS.has(name.slice(lastDot))) return true;
  return false;
}

/**
 * Recursively builds an array of tree-line strings.
 *
 * @param {string}   dirPath   - absolute path of the directory to scan
 * @param {string}   prefix    - current indentation prefix (box-drawing chars)
 * @param {number}   depth     - current recursion depth (0 = root)
 * @returns {string[]}
 */
function buildTree(dirPath, prefix = "", depth = 0) {
  if (MAX_DEPTH !== undefined && depth > MAX_DEPTH) return [];

  let entries;
  try {
    entries = readdirSync(dirPath, { withFileTypes: true });
  } catch {
    return [];
  }

  // Directories first, then files — both sorted alphabetically
  const dirs  = entries.filter(e => e.isDirectory() && !isIgnored(e.name)).sort((a, b) => a.name.localeCompare(b.name));
  const files = entries.filter(e => e.isFile()      && !isIgnored(e.name)).sort((a, b) => a.name.localeCompare(b.name));
  const sorted = [...dirs, ...files];

  const lines = [];

  sorted.forEach((entry, idx) => {
    const isLast      = idx === sorted.length - 1;
    const connector   = isLast ? "└── " : "├── ";
    const childPrefix = isLast ? "    " : "│   ";

    if (entry.isDirectory()) {
      lines.push(`${prefix}${connector}${entry.name}/`);
      const children = buildTree(join(dirPath, entry.name), prefix + childPrefix, depth + 1);
      lines.push(...children);
    } else {
      lines.push(`${prefix}${connector}${entry.name}`);
    }
  });

  return lines;
}

// ── README updater ────────────────────────────────────────────────────────────

function main() {
  const projectRoot = resolve(process.cwd());
  const readmePath  = join(projectRoot, "README.md");

  // ── Build the tree ──────────────────────────────────────────────────────

  // Root-level entries (dirs + files), same rules
  let rootEntries;
  try {
    rootEntries = readdirSync(projectRoot, { withFileTypes: true });
  } catch (err) {
    console.error("❌  Could not read project root:", err.message);
    process.exit(1);
  }

  const rootDirs  = rootEntries.filter(e => e.isDirectory() && !isIgnored(e.name)).sort((a, b) => a.name.localeCompare(b.name));
  const rootFiles = rootEntries.filter(e => e.isFile()      && !isIgnored(e.name)).sort((a, b) => a.name.localeCompare(b.name));
  const rootSorted = [...rootDirs, ...rootFiles];

  const treeLines = [];

  rootSorted.forEach((entry, idx) => {
    const isLast    = idx === rootSorted.length - 1;
    const connector = isLast ? "└── " : "├── ";
    const childPfx  = isLast ? "    " : "│   ";

    if (entry.isDirectory()) {
      treeLines.push(`${connector}${entry.name}/`);
      treeLines.push(...buildTree(join(projectRoot, entry.name), childPfx, 1));
    } else {
      treeLines.push(`${connector}${entry.name}`);
    }
  });

  const treeBlock = "```\n" + treeLines.join("\n") + "\n```";

  // ── Read README ─────────────────────────────────────────────────────────

  let readme;
  try {
    readme = readFileSync(readmePath, "utf8");
  } catch (err) {
    console.error("❌  Could not read README.md:", err.message);
    process.exit(1);
  }


  // Detect and normalize line endings
  const crlf = readme.includes("\r\n");
  const normalized = readme.replace(/\r\n/g, "\n");

  // ── Replace the section ──────────────────────────────────────────────────
  //
  // Strategy: split on lines, find the section heading, then find where the
  // next heading begins (or EOF).  Replace everything in that range with the
  // new tree block, preserving all content outside the section.

  const lines  = normalized.split("\n");
  let startIdx    = -1;          // index of the SECTION_HEADING line
  let endIdx      = lines.length; // index of the next heading line (exclusive)

  for (let i = 0; i < lines.length; i++) {
    if (lines[i] === SECTION_HEADING) {
      startIdx = i;
      continue;
    }
    if (startIdx !== -1 && /^#{1,6} /.test(lines[i])) {
      endIdx = i;
      break;
    }
  }

  const newSectionLines = [SECTION_HEADING, "", treeBlock, ""];

  let updatedLines;
  if (startIdx === -1) {
    // Section heading not found — append it at the end
    console.warn(`⚠️   Section "${SECTION_HEADING}" not found in README.md — appending it.`);
    updatedLines = [...lines, "", ...newSectionLines];
  } else {
    updatedLines = [
      ...lines.slice(0, startIdx),
      ...newSectionLines,
      ...lines.slice(endIdx),
    ];
  }

  // ── Write README ─────────────────────────────────────────────────────────

  // Restore original line endings before writing
  const updated = updatedLines.join("\n");
  const finalContent = crlf ? updated.replace(/\n/g, "\r\n") : updated;

  if (finalContent === readme) {
    console.log("✅  README.md architecture section is already up to date.");
    return;
  }

  try {
    writeFileSync(readmePath, finalContent, "utf8");
  } catch (err) {
    console.error("❌  Could not write README.md:", err.message);
    process.exit(1);
  }

  console.log("✅  README.md architecture section updated successfully.");
}

main();
