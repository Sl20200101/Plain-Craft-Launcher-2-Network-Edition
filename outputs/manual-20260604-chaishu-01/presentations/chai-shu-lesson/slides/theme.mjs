export const C = {
  ink: "#18212F",
  muted: "#667085",
  paper: "#FFFDF8",
  mint: "#DFF6E8",
  mintDark: "#178A5B",
  coral: "#FFE1D8",
  coralDark: "#D45D45",
  amber: "#FFE8A3",
  amberDark: "#9B6B00",
  blue: "#DCEBFF",
  blueDark: "#2E64B6",
  line: "#D5D9E2",
  white: "#FFFFFF",
  transparent: "#00000000",
};

export function addBackground(ctx, slide, tone = C.paper) {
  ctx.addShape(slide, { x: 0, y: 0, w: ctx.W, h: ctx.H, fill: tone, line: ctx.line(C.transparent, 0) });
}

export function addTitle(ctx, slide, title, subtitle = "") {
  ctx.addText(slide, {
    text: title,
    x: 64,
    y: 42,
    w: 760,
    h: 64,
    fontSize: 38,
    bold: true,
    color: C.ink,
    typeface: "Microsoft YaHei UI",
  });
  if (subtitle) {
    ctx.addText(slide, {
      text: subtitle,
      x: 66,
      y: 118,
      w: 760,
      h: 36,
      fontSize: 18,
      color: C.muted,
      typeface: "Microsoft YaHei UI",
    });
  }
}

export function addPage(ctx, slide, n) {
  ctx.addText(slide, {
    text: String(n).padStart(2, "0"),
    x: 1160,
    y: 636,
    w: 80,
    h: 28,
    fontSize: 16,
    color: C.muted,
    align: "right",
    typeface: "Aptos",
  });
}

export function panel(ctx, slide, { x, y, w, h, fill = C.white, stroke = C.line }) {
  return ctx.addShape(slide, {
    x,
    y,
    w,
    h,
    fill,
    line: ctx.line(stroke, 1.4),
  });
}

export function text(ctx, slide, value, x, y, w, h, opts = {}) {
  return ctx.addText(slide, {
    text: value,
    x,
    y,
    w,
    h,
    fontSize: opts.size ?? 24,
    bold: opts.bold ?? false,
    color: opts.color ?? C.ink,
    align: opts.align ?? "left",
    valign: opts.valign ?? "top",
    typeface: opts.face ?? "Microsoft YaHei UI",
    fill: opts.fill ?? C.transparent,
    line: ctx.line(C.transparent, 0),
    insets: opts.insets ?? { left: 0, right: 0, top: 0, bottom: 0 },
  });
}

export function rule(ctx, slide, x, y, w, h, fill = C.line) {
  ctx.addShape(slide, { x, y, w, h, fill, line: ctx.line(C.transparent, 0) });
}

export function numberCard(ctx, slide, value, x, y, w = 96, h = 76, opts = {}) {
  panel(ctx, slide, {
    x,
    y,
    w,
    h,
    fill: opts.fill ?? C.white,
    stroke: opts.stroke ?? C.line,
  });
  text(ctx, slide, String(value), x, y + 7, w, h - 10, {
    size: opts.size ?? 38,
    bold: true,
    align: "center",
    valign: "middle",
    color: opts.color ?? C.ink,
    face: "Aptos Display",
  });
}

export function splitDiagram(ctx, slide, { x, y, total, left, right, accent = C.blueDark }) {
  numberCard(ctx, slide, total, x + 146, y, 116, 88, { fill: C.blue, stroke: accent, color: accent, size: 48 });
  text(ctx, slide, "／      ＼", x + 34, y + 88, 360, 92, {
    size: 54,
    bold: true,
    align: "center",
    color: accent,
    face: "Microsoft YaHei UI",
  });
  numberCard(ctx, slide, left, x + 30, y + 178, 112, 82, { fill: C.mint, stroke: C.mintDark, color: C.mintDark });
  numberCard(ctx, slide, right, x + 282, y + 178, 112, 82, { fill: C.coral, stroke: C.coralDark, color: C.coralDark });
  text(ctx, slide, `${total} 可以分成 ${left} 和 ${right}`, x, y + 294, 424, 44, {
    size: 26,
    bold: true,
    align: "center",
    color: C.ink,
    face: "Microsoft YaHei UI",
  });
}

export function beadRow(ctx, slide, x, y, count, filled, opts = {}) {
  const size = opts.size ?? 34;
  const gap = opts.gap ?? 9;
  for (let i = 0; i < count; i += 1) {
    const isFilled = i < filled;
    ctx.addShape(slide, {
      geometry: "rect",
      x: x + i * (size + gap),
      y,
      w: size,
      h: size,
      fill: isFilled ? opts.fill ?? C.amber : C.white,
      line: ctx.line(isFilled ? opts.stroke ?? C.amberDark : C.line, 1.2),
    });
  }
}

export function pairStrip(ctx, slide, { label, a, b, x, y, color = C.blueDark }) {
  text(ctx, slide, label, x, y + 8, 54, 34, { size: 18, bold: true, color, align: "right", face: "Aptos" });
  numberCard(ctx, slide, a, x + 78, y, 66, 50, { fill: C.white, stroke: C.line, size: 28, color });
  text(ctx, slide, "+", x + 152, y + 8, 30, 34, { size: 24, bold: true, align: "center", color: C.muted, face: "Aptos" });
  numberCard(ctx, slide, b, x + 188, y, 66, 50, { fill: C.white, stroke: C.line, size: 28, color });
}
