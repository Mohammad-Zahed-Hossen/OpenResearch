// OpenResearch UI Enhancements: Pristine Markdown Exporter & Single-Instance Controller
(function () {
  'use strict';
  if (window.__ORX_UX__) return;
  window.__ORX_UX__ = true;

  console.log('[OpenResearch UX] Pristine Markdown active.');

  const svg = (p, c = 'currentColor', w = 2) => '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="' + c + '" stroke-width="' + w + '">' + p + '</svg>';
  const DL = svg('<path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M7 10l5 5 5-5M12 15V3"/>');
  const CP = svg('<rect x="9" y="9" width="13" height="13" rx="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>');
  const OK = svg('<path d="m20 6-11 11-5-5"/>', '#10b981', 2.5);
  const BTN_CLS = 'inline-flex items-center justify-center p-1 rounded hover:text-text hover:bg-panel cursor-pointer transition-colors';

  const mkBtn = (tip, icon, cls = BTN_CLS) => {
    const b = document.createElement('button'); b.type = 'button'; b.className = cls;
    b.title = b.ariaLabel = tip; b.setAttribute('data-tip', tip); b.innerHTML = icon;
    return b;
  };

  function dl(c, fn) {
    const b = new Blob([c], { type: 'text/markdown;charset=utf-8' });
    const u = URL.createObjectURL(b);
    const a = document.createElement('a');
    a.href = u; a.download = fn;
    document.body.appendChild(a); a.click();
    document.body.removeChild(a); URL.revokeObjectURL(u);
  }

  function cp(t) {
    if (navigator.clipboard && window.isSecureContext) return navigator.clipboard.writeText(t);
    const ta = document.createElement('textarea');
    ta.value = t; ta.style.position = 'fixed'; ta.style.opacity = '0';
    document.body.appendChild(ta); ta.focus(); ta.select();
    return new Promise((res, rej) => { document.execCommand('copy') ? res() : rej(); ta.remove(); });
  }

  function pId() { const m = location.pathname.match(/\/projects\/([a-zA-Z0-9_-]+)/); return m ? m[1] : null; }
  function sId() { const m = location.pathname.match(/\/tasks\/([a-zA-Z0-9_-]+)/); return m ? m[1] : null; }
  function cleanStr(s) { return (s || '').replace(/[\u200B-\u200D\u2060-\u2069\uFEFF]/g, '').trim(); }

  function enrichMd(md) {
    if (!md) return '';
    const cb = [], ln = [];
    md = md.replace(/```[\s\S]*?```/g, m => { cb.push(m); return '@@@C' + (cb.length - 1) + '@@@'; });
    md = md.replace(/<file\s+path=["']([^"']+)["']\s*\/?>(?:<\/file>)?/gi, (m, p) => {
      const fn = p.split('/').pop() || p;
      return '📄 **[' + fn + '](' + p + ')**';
    });
    md = md.replace(/\[[^\]]+\]\([^\)]+\)|https?:\/\/[^\s<>"')\]]+/g, m => {
      if (m.startsWith('http')) m = '<' + m + '>';
      ln.push(m); return '@@@L' + (ln.length - 1) + '@@@';
    });
    md = md.replace(/`?\b(10\.\d{4,9}\/[-._;()/:a-zA-Z0-9]+)\b`?/g, (m, d) => {
      const c = d.replace(/[.,;:]$/, '');
      ln.push('[' + c + '](https://doi.org/' + c + ')');
      return '@@@L' + (ln.length - 1) + '@@@';
    });
    md = md.replace(/`?\b(\d{4}\.[a-z]+-[a-z0-9.]+)\b`?/g, (m, a) => {
      const c = a.replace(/[.,;:]$/, '');
      ln.push('[' + c + '](https://aclanthology.org/' + c + '/)');
      return '@@@L' + (ln.length - 1) + '@@@';
    });
    md = md.replace(/`?\b(\d{4}\.\d{4,5}(?:v\d+)?)\b`?/g, (m, a) => {
      ln.push('[' + a + '](https://arxiv.org/abs/' + a + ')');
      return '@@@L' + (ln.length - 1) + '@@@';
    });
    for (let i = ln.length - 1; i >= 0; i--) md = md.replace('@@@L' + i + '@@@', ln[i]);
    for (let i = cb.length - 1; i >= 0; i--) md = md.replace('@@@C' + i + '@@@', cb[i]);
    return md;
  }

  async function dlFileByPath(rawPath, fn) {
    const cl = cleanStr(rawPath), rel = cl.replace(/^artifacts[/]/, '');
    let pid = pId();
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
              if (txt && !txt.startsWith('{"error":')) {
                dl(fn.endsWith('.md') ? enrichMd(txt) : txt, fn);
                return true;
              }
            }
          } catch (x) {}
        }
      }
    }
    return false;
  }

  function domMd(n) {
    const c = n.cloneNode(true);
    c.querySelectorAll('.turn-work,.orx-bar,.orx-btn,.orx-chip-dl,button,[data-tip]').forEach(e => e.remove());
    c.querySelectorAll('table').forEach(tbl => {
      let out = '\n\n', rows = Array.from(tbl.querySelectorAll('tr'));
      rows.forEach((r, i) => {
        const cells = Array.from(r.querySelectorAll('th,td'));
        out += '| ' + cells.map(x => x.innerText.replace(/\r?\n+/g, ' ').trim()).join(' | ') + ' |\n';
        if (i === 0) out += '| ' + cells.map(() => '---').join(' | ') + ' |\n';
      });
      tbl.replaceWith(out + '\n');
    });
    c.querySelectorAll('a').forEach(a => { const h = a.getAttribute('href'); if (h) a.replaceWith('[' + (a.innerText.trim() || h) + '](' + h + ')'); });
    c.querySelectorAll('pre').forEach(p => { const cd = p.querySelector('code'), l = cd ? (cd.className.match(/language-(\w+)/) || ['', ''])[1] : ''; p.replaceWith('\n```' + l + '\n' + (cd ? cd.innerText : p.innerText).trim() + '\n```\n'); });
    c.querySelectorAll('code').forEach(cd => { if (!cd.closest('pre')) cd.replaceWith('`' + cd.innerText.trim() + '`'); });
    ['h1','h2','h3','h4'].forEach((t, i) => c.querySelectorAll(t).forEach(h => h.replaceWith('\n' + '#'.repeat(i + 1) + ' ' + h.innerText.trim() + '\n\n')));
    c.querySelectorAll('strong,b').forEach(s => s.replaceWith('**' + s.innerText.trim() + '**'));
    c.querySelectorAll('em,i').forEach(em => em.replaceWith('*' + em.innerText.trim() + '*'));
    c.querySelectorAll('li').forEach(l => l.replaceWith('\n- ' + l.innerText.trim()));
    return c.innerText.replace(/^Worked for [^\n]+\n+/gm, '').replace(/^Used tools[^\n]*\n+/gm, '').trim();
  }

  async function getMsgMd(mId, node) {
    const s = sId();
    if (s) {
      try {
        const r = await fetch('/api/chat/sessions/' + encodeURIComponent(s) + '/messages');
        if (r.ok) {
          const d = await r.json();
          if (d && d.messages) {
            const as = d.messages.filter(x => x.role === 'assistant');
            let m = mId ? as.find(x => x.id === mId) : null;
            if (!m && node) {
              const allAs = Array.from(document.querySelectorAll('.msg-assistant'));
              const idx = allAs.indexOf(node);
              if (idx >= 0 && idx < as.length) m = as[idx];
            }
            if (!m) m = as[as.length - 1];
            if (m && m.parts) {
              const txts = m.parts.filter(p => p.type === 'text' && p.text && p.text.trim().length > 0).map(p => p.text.trim());
              if (txts.length) return enrichMd(txts.join('\n\n'));
            }
          }
        }
      } catch (e) {}
    }
    return enrichMd(domMd(node));
  }

  function injFv() {
    const fv = document.querySelector('.file-view'); if (!fv) return;
    const hdr = fv.querySelector('.file-view-header'); if (!hdr) return;
    const ex = hdr.querySelectorAll('.orx-fv-dl'); if (ex.length > 1) for (let i = 1; i < ex.length; i++) ex[i].remove();
    if (ex.length === 1) return;

    let fp = ''; const pe = hdr.querySelector('.file-view-path'); if (pe) fp = cleanStr(pe.getAttribute('data-tip') || pe.innerText);
    if (!fp) { try { const p = new URLSearchParams(location.search).get('pane'); if (p) fp = cleanStr(JSON.parse(p).path || ''); } catch (e) {} }
    const fn = fp ? fp.split('/').pop() : 'document.md';

    const b = mkBtn('Download ' + fn, DL + '<span>Download ' + (fn.endsWith('.md') ? 'Markdown' : 'File') + '</span>', 'orx-fv-dl inline-flex items-center gap-1.5 py-1 px-2.5 rounded-sm border border-border-variant bg-surface text-text hover:border-text text-xs font-medium cursor-pointer shrink-0');
    b.onclick = async (e) => {
      e.stopPropagation(); b.innerHTML = OK + '<span>Downloading...</span>';
      try {
        let ok = await dlFileByPath(fp, fn);
        if (!ok) {
          const body = fv.querySelector('.fpreview-body');
          if (body) { const pr = body.querySelector('pre'); const t = pr ? pr.innerText : domMd(body); if (t) { dl(fn.endsWith('.md') ? enrichMd(t) : t, fn); ok = true; } }
        }
        b.innerHTML = ok ? (OK + '<span>Downloaded!</span>') : (DL + '<span>Download</span>');
      } catch (err) { b.innerHTML = DL + '<span>Download</span>'; }
      setTimeout(() => b.innerHTML = DL + '<span>Download ' + (fn.endsWith('.md') ? 'Markdown' : 'File') + '</span>', 2500);
    };
    const last = hdr.querySelector('button[aria-label*="panel" i], button[title*="panel" i]');
    if (last && last.parentElement === hdr) hdr.insertBefore(b, last); else hdr.appendChild(b);
  }

  function injChips() {
    document.querySelectorAll('.file-chip').forEach(ch => {
      ch.querySelectorAll('.orx-btn').forEach(e => e.remove());
      const next = ch.nextElementSibling;
      if (next && next.classList.contains('orx-chip-dl')) return;

      const tit = cleanStr(ch.getAttribute('title') || ch.getAttribute('aria-label') || '');
      const m = tit.match(/Open\s+([^\s]+)\s+in/i);
      const rawPath = m ? m[1] : cleanStr((ch.querySelector('.file-chip-label') || ch).innerText);
      const fn = rawPath.split('/').pop() || 'file.md';

      const b = mkBtn('Download ' + fn, DL, 'orx-chip-dl inline-flex items-center justify-center ms-1 p-1 rounded hover:bg-surface border border-border-variant/60 text-subtext hover:text-text cursor-pointer opacity-80 hover:opacity-100 transition-colors align-middle');
      b.onclick = async (e) => {
        e.stopPropagation(); e.preventDefault();
        b.innerHTML = OK;
        const ok = await dlFileByPath(rawPath, fn);
        if (!ok) {
          ch.click();
          setTimeout(() => {
            const fvDl = document.querySelector('.orx-fv-dl');
            if (fvDl) fvDl.click();
          }, 400);
        }
        setTimeout(() => b.innerHTML = DL, 2000);
      };
      ch.insertAdjacentElement('afterend', b);
    });
  }

  function injMsgs() {
    document.querySelectorAll('.msg-assistant').forEach((msg, idx) => {
      const ex = msg.querySelectorAll('.orx-bar');
      if (ex.length > 1) for (let i = 1; i < ex.length; i++) ex[i].remove();
      if (ex.length === 1) {
        if (ex[0] !== msg.lastElementChild) msg.appendChild(ex[0]);
        return;
      }
      const pos = window.getComputedStyle(msg).position; if (pos === 'static') msg.style.position = 'relative';

      const bar = document.createElement('div');
      bar.className = 'orx-bar flex items-center gap-1 mt-3 mb-1 p-0.5 rounded-md bg-surface/80 border border-border-variant/60 w-fit text-subtext select-none';
      const mw = msg.closest('[data-message-id]'); const mId = mw ? mw.getAttribute('data-message-id') : null;

      const cpb = mkBtn('Copy Response', CP);
      cpb.onclick = async (e) => {
        e.stopPropagation();
        const txt = await getMsgMd(mId, msg);
        cp(txt).then(() => { cpb.innerHTML = OK; setTimeout(() => cpb.innerHTML = CP, 2000); });
      };

      const dlb = mkBtn('Export Markdown (.md)', DL);
      dlb.onclick = async (e) => {
        e.stopPropagation();
        const txt = await getMsgMd(mId, msg);
        const te = document.querySelector('header h1, header h2, .chat-thread h1') || document.querySelector('title');
        let bn = te ? te.innerText.trim().replace(/\s*—\s*OpenResearch/i, '') : 'research';
        bn = bn.replace(/[^a-z0-9_-]/gi, '-').toLowerCase() || 'response';
        dl(txt, bn + '-turn-' + (idx + 1) + '.md');
        dlb.innerHTML = OK;
        setTimeout(() => dlb.innerHTML = DL, 2000);
      };

      bar.appendChild(cpb);
      bar.appendChild(dlb);
      msg.appendChild(bar);
    });
  }

  function injCode() {
    document.querySelectorAll('pre').forEach(p => {
      const ex = p.querySelectorAll('.orx-cp-code'); if (ex.length > 1) for (let i = 1; i < ex.length; i++) ex[i].remove();
      if (ex.length === 1) return;
      const pos = window.getComputedStyle(p).position; if (pos === 'static') p.style.position = 'relative';
      const b = mkBtn('Copy code', CP, 'orx-cp-code absolute top-2 right-2 ' + BTN_CLS + ' bg-surface/90 border border-border-variant text-xs opacity-70 hover:opacity-100 z-10');
      b.onclick = (e) => {
        e.stopPropagation(); const cd = p.querySelector('code') || p;
        cp(cd.innerText).then(() => { b.innerHTML = OK; setTimeout(() => b.innerHTML = CP, 1800); });
      };
      p.appendChild(b);
    });
  }

  function injTop() {
    if (document.getElementById('orx-top-dl')) return;
    const cands = document.querySelectorAll('[aria-label="Files"], [aria-label="Artifacts"], [aria-label="Terminal"], [aria-label="Experiments"]');
    if (!cands.length) return;
    const ref = cands[0], parent = ref.parentElement; if (!parent) return;
    const b = mkBtn('Export Full Session (.md)', DL, ref.className);
    b.id = 'orx-top-dl';
    b.onclick = async () => {
      const te = document.querySelector('header h1, header h2, .chat-thread h1') || document.querySelector('title');
      let title = te ? te.innerText.trim().replace(/\s*—\s*OpenResearch/i, '') : 'OpenResearch-Output';
      b.innerHTML = OK;
      let out = '# ' + title + '\n\n*Exported ' + new Date().toLocaleString() + '*\n\n---\n\n';
      const s = sId(); let fetched = false;
      if (s) {
        try {
          const r = await fetch('/api/chat/sessions/' + encodeURIComponent(s) + '/messages');
          if (r.ok) {
            const d = await r.json();
            if (d && d.messages) {
              const as = d.messages.filter(m => m.role === 'assistant');
              const parts = [];
              as.forEach(m => {
                const t = m.parts.filter(p => p.type === 'text' && p.text && p.text.trim().length > 0).map(p => p.text.trim()).join('\n\n');
                if (t) parts.push(t);
              });
              if (parts.length) { out += enrichMd(parts.join('\n\n---\n\n')); fetched = true; }
            }
          }
        } catch (e) {}
      }
      if (!fetched) {
        const parts = []; document.querySelectorAll('.msg-assistant').forEach(m => { const t = domMd(m); if (t) parts.push(t); });
        out += enrichMd(parts.join('\n\n---\n\n'));
      }
      const sfn = title.replace(/[^a-z0-9_\-\s]/gi, '').replace(/\s+/g, '-').toLowerCase();
      dl(out, sfn + '.md');
      setTimeout(() => b.innerHTML = DL, 1800);
    };
    parent.appendChild(b);
  }

  function run() { injTop(); injFv(); injChips(); injMsgs(); injCode(); }
  run();
  let t = null;
  const mo = new MutationObserver(() => { if (t) clearTimeout(t); t = setTimeout(run, 120); });
  mo.observe(document.body, { childList: true, subtree: true });
  window.addEventListener('scroll', () => { if (t) clearTimeout(t); t = setTimeout(run, 120); }, { capture: true, passive: true });
})();
