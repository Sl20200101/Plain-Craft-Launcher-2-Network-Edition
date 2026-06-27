import { C, addBackground, addPage, addTitle, splitDiagram, text } from "./theme.mjs";

export async function slide02(presentation, ctx) {
  const slide = presentation.slides.add();
  addBackground(ctx, slide);
  addTitle(ctx, slide, "示例：3 可以分成 1 和 2", "只要拆成两个数即可。");

  splitDiagram(ctx, slide, { x: 190, y: 178, total: 3, left: 1, right: 2, accent: C.coralDark });
  text(ctx, slide, "先写 3", 766, 214, 230, 38, { size: 29, bold: true, color: C.ink });
  text(ctx, slide, "在 3 的下面画一撇一捺", 766, 300, 360, 38, { size: 26, color: C.ink });
  text(ctx, slide, "下面写 1 和 2", 766, 386, 280, 38, { size: 26, color: C.ink });
  text(ctx, slide, "读作：3 可以分成 1 和 2", 766, 490, 380, 42, { size: 26, bold: true, color: C.coralDark });

  addPage(ctx, slide, 2);
  return slide;
}
