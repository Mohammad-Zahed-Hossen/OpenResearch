# OpenResearch UI Enhancements Auto-Patcher
# Automatically patches orx.exe to restore Copy and Download buttons after app updates.

param(
    [string]$OrxPath = "$PSScriptRoot\orx.exe",
    [string]$ExtPath = "$PSScriptRoot\extension\content.js"
)

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " OpenResearch UI Enhancements Patcher (Persistent Fix)   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

if (-not (Test-Path $OrxPath)) {
    Write-Host "[ERROR] orx.exe not found at: $OrxPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $ExtPath)) {
    Write-Host "[ERROR] extension\content.js not found at: $ExtPath" -ForegroundColor Red
    exit 1
}

# 1. Stop running orx processes to unlock the executable
$running = Get-Process -Name "orx" -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "[INFO] Stopping active orx.exe process to allow updating..." -ForegroundColor Yellow
    Stop-Process -Name "orx" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 800
}

# 2. C# In-Place Region Patcher definition
$Source = @"
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class OrxPatcher
{
    public static int Patch(string orxPath, string extPath)
    {
        try
        {
            if (!File.Exists(orxPath) || !File.Exists(extPath)) return -1;

            string uxRaw = File.ReadAllText(extPath, Encoding.UTF8);
            string uxMin = Regex.Replace(uxRaw, @"(?m)^\s*//.*$", "");
            uxMin = Regex.Replace(uxMin, @"\s+", " ").Trim();
            byte[] uxBytes = Encoding.UTF8.GetBytes(uxMin);

            byte[] data = File.ReadAllBytes(orxPath);
            byte[] beforeMarker = Encoding.UTF8.GetBytes("document.body)}/*ISC*/");
            byte[] afterMarker = Encoding.UTF8.GetBytes("const iI=(...e)=>e.filter");
            int beforeIdx = IndexOfBytes(data, beforeMarker, 0);
            int afterIdx = IndexOfBytes(data, afterMarker, 0);

            if (beforeIdx != -1 && afterIdx != -1 && afterIdx > beforeIdx)
            {
                int patchStart = beforeIdx + beforeMarker.Length;
                int patchLen = afterIdx - patchStart;
                if (patchLen == 13940)
                {
                    int deficit = patchLen - uxBytes.Length;
                    if (deficit >= 4)
                    {
                        string pad = "/*" + new string(' ', deficit - 4) + "*/";
                        byte[] padBytes = Encoding.ASCII.GetBytes(pad);
                        Buffer.BlockCopy(uxBytes, 0, data, patchStart, uxBytes.Length);
                        Buffer.BlockCopy(padBytes, 0, data, patchStart + uxBytes.Length, padBytes.Length);
                        File.WriteAllBytes(orxPath, data);
                        return 1; // Updated successfully
                    }
                    return -3;
                }
            }

            const string targetCommentStr = "/**\n * @license lucide-react v1.23.0 - ISC\n *\n * This source code is licensed under the ISC license.\n * See the LICENSE file in the root directory of this source tree.\n */";
            byte[] targetComment = Encoding.UTF8.GetBytes(targetCommentStr);
            const string replacementStr = "/*ISC*/";

            List<int> matches = new List<int>();
            int pos = 0;
            while (true)
            {
                int idx = IndexOfBytes(data, targetComment, pos);
                if (idx == -1) break;
                matches.Add(idx);
                pos = idx + targetComment.Length;
            }

            if (matches.Count == 0) return -2; // Comments not found

            int minPos = matches[0];
            int maxPos = matches[matches.Count - 1] + targetComment.Length;
            int regionLen = maxPos - minPos;

            byte[] origRegion = new byte[regionLen];
            Buffer.BlockCopy(data, minPos, origRegion, 0, regionLen);
            string regionStr = Encoding.UTF8.GetString(origRegion);

            string replacedStr = regionStr.Replace(targetCommentStr, replacementStr);
            byte[] replacedBytes = Encoding.UTF8.GetBytes(replacedStr);

            int spaceSaved = origRegion.Length - replacedBytes.Length;
            int deficitSpace = spaceSaved - uxBytes.Length;
            if (deficitSpace < 4) return -3; // Not enough space

            string padSpace = "/*" + new string(' ', deficitSpace - 4) + "*/";
            int firstIsc = replacedStr.IndexOf(replacementStr);
            if (firstIsc == -1) return -4;

            string finalRegionStr = replacedStr.Substring(0, firstIsc + replacementStr.Length)
                                  + uxMin
                                  + padSpace
                                  + replacedStr.Substring(firstIsc + replacementStr.Length);

            byte[] newRegionBytes = Encoding.UTF8.GetBytes(finalRegionStr);
            if (newRegionBytes.Length != regionLen) return -5; // Length mismatch

            Buffer.BlockCopy(newRegionBytes, 0, data, minPos, regionLen);

            // Bust browser cache in index.html so web app immediately pulls fresh JS
            try
            {
                byte[] htmlTag = Encoding.UTF8.GetBytes("<!doctype html>");
                int htmlPos = IndexOfBytes(data, htmlTag, 0);
                if (htmlPos != -1)
                {
                    int htmlEnd = IndexOfBytes(data, Encoding.UTF8.GetBytes("</html>"), htmlPos);
                    if (htmlEnd != -1)
                    {
                        int hLen = (htmlEnd + 7) - htmlPos;
                        byte[] hBytes = new byte[hLen];
                        Buffer.BlockCopy(data, htmlPos, hBytes, 0, hLen);
                        string hStr = Encoding.UTF8.GetString(hBytes);
                        const string oldC = "/* Pre-CSS background; keep in sync with --base in src/tailwind.css. */";
                        const string newC = "/* Pre-CSS background; keep in sync with --base in tailwind.css. */";
                        if (hStr.Contains(oldC))
                        {
                            string newH = hStr.Replace(oldC, newC);
                            newH = Regex.Replace(newH, @"(src=""/assets/index-[^""]+\.js)""", "$1?v=2\"");
                            byte[] newHBytes = Encoding.UTF8.GetBytes(newH);
                            if (newHBytes.Length == hLen)
                            {
                                Buffer.BlockCopy(newHBytes, 0, data, htmlPos, hLen);
                            }
                        }
                    }
                }
            }
            catch { }

            string bak = orxPath + ".orig";
            if (!File.Exists(bak))
            {
                try { File.Copy(orxPath, bak); } catch { }
            }

            File.WriteAllBytes(orxPath, data);
            return 1; // Patched successfully
        }
        catch (Exception)
        {
            return -1; // Exception
        }
    }

    private static int IndexOfBytes(byte[] haystack, byte[] needle, int startIndex)
    {
        if (needle.Length == 0 || haystack.Length < needle.Length) return -1;
        int max = haystack.Length - needle.Length;
        for (int i = startIndex; i <= max; i++)
        {
            if (haystack[i] == needle[0])
            {
                bool match = true;
                for (int j = 1; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
        }
        return -1;
    }
}
"@

Add-Type -TypeDefinition $Source -Language CSharp

$res = [OrxPatcher]::Patch((Resolve-Path $OrxPath).Path, (Resolve-Path $ExtPath).Path)

switch ($res) {
    0 {
        Write-Host "[OK] orx.exe is ALREADY patched with UI enhancements (__ORX_UX__ is present)." -ForegroundColor Green
    }
    1 {
        Write-Host "[SUCCESS] orx.exe successfully patched with UI enhancements!" -ForegroundColor Green
        Write-Host "  - Copy Response button restored" -ForegroundColor Green
        Write-Host "  - Markdown Export (.md) button restored" -ForegroundColor Green
        Write-Host "  - Deliverable File Downloader restored" -ForegroundColor Green
    }
    -1 { Write-Host "[ERROR] Failed to patch orx.exe: An exception occurred (check permissions/lock)." -ForegroundColor Red }
    -2 { Write-Host "[ERROR] Could not find lucide-react comment anchors in binary." -ForegroundColor Red }
    -3 { Write-Host "[ERROR] Not enough reclaimable comment space in JS chunk." -ForegroundColor Red }
    default { Write-Host "[ERROR] Patch failed with code: $res" -ForegroundColor Red }
}
