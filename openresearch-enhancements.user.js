// ==UserScript==
// @name         OpenResearch UI Enhancements (Copy & Markdown File Downloader)
// @namespace    https://openresearch.sh
// @version      1.3.0
// @description  Single-instance File Viewer Downloader, Message Copy/Export, Code Copy, and Chip Downloaders.
// @author       Antigravity
// @match        http://127.0.0.1:4791/*
// @match        http://localhost:4791/*
// @grant        none
// @run-at       document-idle
// ==/UserScript==

(function () {
  'use strict';
  if (window.__ORX_UX__) return;
  window.__ORX_UX__ = true;

  console.log('[OpenResearch UX] Single-instance userscript active.');

  const DL_ICON = '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>';
  const CP_ICON = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path></svg>';
  const OK_ICON = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#10b981" stroke-width="2.5"><polyline points="20 6 9 17 4 12"></polyline></svg>';

  function dlBlob(content, filename) {
    const b = new Blob([content], { type: 'text/markdown;charset=utf-8' });
    const u = URL.createObjectURL(b);
    const a = document.createElement('a');
    a.href = u; a.download = filename;
    document.body.appendChild(a); a.click();
    document.body.removeChild(a); URL.revokeObjectURL(u);
  }

  function copyText(t) {
    if (navigator.clipboard && window.isSecureContext) return navigator.clipboard.writeText(t);
    const ta = document.createElement('textarea');
    ta.value = t; ta.style.position = 'fixed'; ta.style.opacity = '0';
    document.body.appendChild(ta); ta.focus(); ta.select();
    return new Promise((res, rej) => { document.execCommand('copy') ? res() : rej(); ta.remove(); });
  }

  function getProjId() {
    const m = window.location.pathname.match(/\/projects\/([a-zA-Z0-9_-]+)/);
    return m ? m[1] : null;
  }

  function cleanMd(node) {
    const c = node.cloneNode(true);
    c.querySelectorAll('.turn-work,.orx-bar,.orx-msg-actions-bar,.orx-btn,.orx-chip-dl-btn,button,[data-tip]').forEach(e => e.remove());
    c.querySelectorAll('pre').forEach(p => {
      const cd = p.querySelector('code');
      const lang = cd ? (cd.className.match(/language-(\w+)/) || ['', ''])[1] : '';
      p.replaceWith('\n```' + lang + '\n' + (cd ? cd.innerText : p.innerText).trim() + '\n```\n');
    });
    c.querySelectorAll('h1').forEach(h => h.replaceWith('\n# ' + h.innerText.trim() + '\n'));
    c.querySelectorAll('h2').forEach(h => h.replaceWith('\n## ' + h.innerText.trim() + '\n'));
    c.querySelectorAll('h3').forEach(h => h.replaceWith('\n### ' + h.innerText.trim() + '\n'));
    c.querySelectorAll('h4').forEach(h => h.replaceWith('\n#### ' + h.innerText.trim() + '\n'));
    c.querySelectorAll('li').forEach(l => l.replaceWith('\n- ' + l.innerText.trim()));
    return c.innerText.replace(/^Worked for [^\n]+\n+/gm, '').replace(/^Used tools[^\n]*\n+/gm, '').trim();
  }

  function cleanStr(s) { return (s || '').replace(/[\u200B-\u200D\u2060-\u2069\uFEFF]/g, '').trim(); }

  async function dlFileByPath(rawPath, fn) {
    const cl = cleanStr(rawPath), rel = cl.replace(/^artifacts\//, '');
    let pid = null;
    const m = location.pathname.match(/\/projects\/([a-zA-Z0-9_-]+)/);
    if (m) pid = m[1];
    if (!pid) {
      try {
        const pr = await fetch('/api/projects');
        if (pr.ok) { const pd = await pr.json(); if (pd && pd.projects && pd.projects.length) pid = pd.projects[0].id; }
      } catch (e) {}
    }
    if (pid) {
      for (const p of [rel, cl, 'artifacts/' + rel]) {
        for (const u of ['/api/projects/' + pid + '/files/file?path=' + encodeURIComponent(p), '/api/projects/' + pid + '/file/raw?path=' + encodeURIComponent(p)]) {
          try {
            const res = await fetch(u);
            if (res.ok) {
              const txt = await res.text();
              if (txt && !txt.startsWith('{"error":')) { dlBlob(txt, fn); return true; }
            }
          } catch (x) {}
        }
      }
    }
    return false;
  }

  function injectFileViewer() {
    const fv = document.querySelector('.file-view');
    if (!fv) return;
    const header = fv.querySelector('.file-view-header');
    if (!header) return;

    const existing = header.querySelectorAll('.orx-fv-dl');
    if (existing.length > 1) {
      for (let i = 1; i < existing.length; i++) existing[i].remove();
    }
    if (existing.length === 1) return;

    let filePath = '';
    const pathEl = header.querySelector('.file-view-path');
    if (pathEl) filePath = cleanStr(pathEl.getAttribute('data-tip') || pathEl.innerText);
    if (!filePath) {
      try {
        const pane = new URLSearchParams(window.location.search).get('pane');
        if (pane) filePath = cleanStr(JSON.parse(pane).path || '');
      } catch (e) {}
    }
    const fileName = filePath ? filePath.split('/').pop() : 'document.md';

    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'orx-fv-dl inline-flex items-center gap-1.5 py-1 px-2.5 rounded-sm border border-border-variant bg-surface text-text hover:border-text text-xs font-medium cursor-pointer shrink-0';
    btn.setAttribute('data-tip', 'Download ' + fileName);
    btn.innerHTML = DL_ICON + '<span>Download ' + (fileName.endsWith('.md') ? 'Markdown' : 'File') + '</span>';

    btn.onclick = async (e) => {
      e.stopPropagation();
      btn.innerHTML = OK_ICON + '<span>Downloading...</span>';
      try {
        let ok = await dlFileByPath(filePath, fileName);
        if (!ok) {
          const body = fv.querySelector('.fpreview-body');
          if (body) {
            const pre = body.querySelector('pre');
            const txt = pre ? pre.innerText : cleanMd(body);
            if (txt) { dlBlob(txt, fileName); ok = true; }
          }
        }
        btn.innerHTML = ok ? (OK_ICON + '<span>Downloaded!</span>') : (DL_ICON + '<span>Download</span>');
      } catch (err) {
        console.error('Download error:', err);
        btn.innerHTML = DL_ICON + '<span>Download</span>';
      }
      setTimeout(() => {
        btn.innerHTML = DL_ICON + '<span>Download ' + (fileName.endsWith('.md') ? 'Markdown' : 'File') + '</span>';
      }, 2500);
    };

    const panelClose = header.querySelector('button[aria-label*="panel" i], button[title*="panel" i]');
    if (panelClose && panelClose.parentElement === header) {
      header.insertBefore(btn, panelClose);
    } else {
      header.appendChild(btn);
    }
  }

  function injectChips() {
    document.querySelectorAll('.file-chip').forEach(ch => {
      ch.querySelectorAll('.orx-btn').forEach(e => e.remove());
      const next = ch.nextElementSibling;
      if (next && next.classList.contains('orx-chip-dl')) return;

      const tit = cleanStr(ch.getAttribute('title') || ch.getAttribute('aria-label') || '');
      const m = tit.match(/Open\s+([^\s]+)\s+in/i);
      const rawPath = m ? m[1] : cleanStr((ch.querySelector('.file-chip-label') || ch).innerText);
      const fn = rawPath.split('/').pop() || 'file.md';

      const b = document.createElement('button');
      b.type = 'button';
      b.className = 'orx-chip-dl inline-flex items-center justify-center ms-1 p-1 rounded hover:bg-surface border border-border-variant/60 text-subtext hover:text-text cursor-pointer opacity-80 hover:opacity-100 transition-colors align-middle';
      b.setAttribute('title', 'Download ' + fn);
      b.setAttribute('data-tip', 'Download ' + fn);
      b.setAttribute('aria-label', 'Download ' + fn);
      b.innerHTML = DL_ICON;

      b.onclick = async (e) => {
        e.stopPropagation();
        e.preventDefault();
        b.innerHTML = OK_ICON;
        const ok = await dlFileByPath(rawPath, fn);
        if (!ok) {
          ch.click();
          setTimeout(() => {
            const fvDl = document.querySelector('.orx-fv-dl');
            if (fvDl) fvDl.click();
          }, 400);
        }
        setTimeout(() => b.innerHTML = DL_ICON, 2000);
      };

      ch.insertAdjacentElement('afterend', b);
    });
  }

  function injectMsgs() {
    document.querySelectorAll('.msg-assistant').forEach((msg, idx) => {
      const existing = msg.querySelectorAll('.orx-bar, .orx-msg-actions-bar');
      if (existing.length > 1) {
        for (let i = 1; i < existing.length; i++) existing[i].remove();
      }
      if (existing.length === 1) {
        if (existing[0] !== msg.lastElementChild) msg.appendChild(existing[0]);
        return;
      }

      const pos = window.getComputedStyle(msg).position;
      if (pos === 'static') msg.style.position = 'relative';

      const bar = document.createElement('div');
      bar.className = 'orx-bar flex items-center gap-1 mt-3 mb-1 p-0.5 rounded-md bg-surface/80 border border-border-variant/60 w-fit text-subtext select-none';

      const cp = document.createElement('button');
      cp.type = 'button';
      cp.className = 'inline-flex items-center justify-center p-1 rounded hover:text-text hover:bg-panel cursor-pointer transition-colors';
      cp.setAttribute('title', 'Copy Response');
      cp.setAttribute('data-tip', 'Copy Response');
      cp.setAttribute('aria-label', 'Copy Response');
      cp.innerHTML = CP_ICON;
      cp.onclick = (e) => {
        e.stopPropagation();
        copyText(cleanMd(msg)).then(() => {
          cp.innerHTML = OK_ICON;
          setTimeout(() => cp.innerHTML = CP_ICON, 2000);
        });
      };

      const dl = document.createElement('button');
      dl.type = 'button';
      dl.className = 'inline-flex items-center justify-center p-1 rounded hover:text-text hover:bg-panel cursor-pointer transition-colors';
      dl.setAttribute('title', 'Export Markdown (.md)');
      dl.setAttribute('data-tip', 'Export Markdown (.md)');
      dl.setAttribute('aria-label', 'Export Markdown (.md)');
      dl.innerHTML = DL_ICON;
      dl.onclick = (e) => {
        e.stopPropagation();
        const te = document.querySelector('header h1, header h2, .chat-thread h1') || document.querySelector('title');
        let bn = te ? te.innerText.trim().replace(/\s*—\s*OpenResearch/i, '') : 'research';
        bn = bn.replace(/[^a-z0-9_-]/gi, '-').toLowerCase() || 'response';
        dlBlob(cleanMd(msg), bn + '-turn-' + (idx + 1) + '.md');
        dl.innerHTML = OK_ICON;
        setTimeout(() => dl.innerHTML = DL_ICON, 2000);
      };

      bar.appendChild(cp);
      bar.appendChild(dl);
      msg.appendChild(bar);
    });
  }

  function injectCode() {
    document.querySelectorAll('pre').forEach(p => {
      const existing = p.querySelectorAll('.orx-cp-code, .orx-code-copy-btn');
      if (existing.length > 1) {
        for (let i = 1; i < existing.length; i++) existing[i].remove();
      }
      if (existing.length === 1) return;

      const pos = window.getComputedStyle(p).position;
      if (pos === 'static') p.style.position = 'relative';

      const b = document.createElement('button');
      b.type = 'button';
      b.className = 'orx-cp-code absolute top-2 right-2 inline-flex items-center justify-center p-1 rounded bg-surface/90 border border-border-variant text-subtext hover:text-text text-xs cursor-pointer opacity-70 hover:opacity-100 z-10';
      b.setAttribute('title', 'Copy code');
      b.innerHTML = CP_ICON;
      b.onclick = (e) => {
        e.stopPropagation();
        const cd = p.querySelector('code') || p;
        copyText(cd.innerText).then(() => {
          b.innerHTML = OK_ICON;
          setTimeout(() => b.innerHTML = CP_ICON, 1800);
        });
      };
      p.appendChild(b);
    });
  }

  function injectTop() {
    if (document.getElementById('orx-top-dl')) return;
    const cands = document.querySelectorAll('[aria-label="Files"], [aria-label="Artifacts"], [aria-label="Terminal"], [aria-label="Experiments"]');
    if (cands.length === 0) return;
    const ref = cands[0];
    const parent = ref.parentElement;
    if (!parent) return;

    const b = document.createElement('button');
    b.id = 'orx-top-dl';
    b.type = 'button';
    b.className = ref.className;
    b.setAttribute('aria-label', 'Export Full Session (.md)');
    b.setAttribute('data-tip', 'Export Session (.md)');
    b.innerHTML = DL_ICON;
    b.onclick = () => {
      const te = document.querySelector('header h1, header h2, .chat-thread h1') || document.querySelector('title');
      let title = te ? te.innerText.trim().replace(/\s*—\s*OpenResearch/i, '') : 'OpenResearch-Output';
      const msgs = document.querySelectorAll('.msg-assistant');
      if (msgs.length === 0) return alert('No assistant turns found.');
      let out = '# ' + title + '\n\n*Exported ' + new Date().toLocaleString() + '*\n\n---\n\n';
      let cnt = 0;
      msgs.forEach(m => {
        const md = cleanMd(m);
        if (md) { cnt++; if (cnt > 1) out += '\n\n---\n\n'; out += md + '\n'; }
      });
      const sfn = title.replace(/[^a-z0-9_\-\s]/gi, '').replace(/\s+/g, '-').toLowerCase();
      dlBlob(out, sfn + '.md');
      b.innerHTML = OK_ICON;
      setTimeout(() => b.innerHTML = DL_ICON, 1800);
    };
    parent.appendChild(b);
  }

  function run() {
    injectTop();
    injectFileViewer();
    injectChips();
    injectMsgs();
    injectCode();
  }

  run();
  let t = null;
  const mo = new MutationObserver(() => {
    if (t) clearTimeout(t);
    t = setTimeout(run, 120);
  });
  mo.observe(document.body, { childList: true, subtree: true });
  window.addEventListener('scroll', () => {
    if (t) clearTimeout(t);
    t = setTimeout(run, 120);
  }, { capture: true, passive: true });
})();
