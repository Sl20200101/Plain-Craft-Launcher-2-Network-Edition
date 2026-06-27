import { C, addBackground, addPage, addTitle, numberCard, panel, text } from "./theme.mjs";

export async function slide04(presentation, ctx) {
  const slide = presentation.slides.add();
  addBackground(ctx, slide);
  addTitle(ctx, slide, "请你拆一拆", "在数字下面画一撇一捺，再写出两个数。");

  const items = [
    { n: 6, x: 120, y: 202 },
    { n: 7, x: 520, y: 202 },
    { n: 8, x: 920, y: 202 },
  ];
  for (const item of items) {
    panel(ctx, slide, { x: item.x - 34, y: item.y - 42, w: 270, h: 390, fill: C.white, stroke: C.line });
    numberCard(ctx, slide, item.n, item.x + 72, item.y, 92, 72, { fill: C.blue, stroke: C.blueDark, color: C.blueDark, size: 38 });
    text(ctx, slide, "／    ＼", item.x + 18, item.y + 86, 190, 72, {
      size: 44,
      bold: true,
      align: "center",
      color: C.blueDark,
      face: "Microsoft YaHei UI",
    });
    panel(ctx, slide, { x: item.x, y: item.y + 180, w: 72, h: 60, fill: "#F8FAFC", stroke: C.line });
    panel(ctx, slide, { x: item.x + 156, y: item.y + 180, w: 72, h: 60, fill: "#F8FAFC", stroke: C.line });
    text(ctx, slide, "?", item.x, item.y + 190, 72, 40, { size: 28, bold: true, align: "center", color: C.muted, face: "Aptos Display" });
    text(ctx, slide, "?", item.x + 156, item.y + 190, 72, 40, { size: 28, bold: true, align: "center", color: C.muted, face: "Aptos Display" });
  }

  addPage(ctx, slide, 4);
  return slide;
}
