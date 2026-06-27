# Debug Session: game-cursor-hidden

Status: [OPEN]

## Symptom
- 启动器窗口不关闭时，进入游戏后光标不显示。
- 不能通过关闭启动器来规避。

## Hypotheses
- H1: Minecraft 窗口出现后仍未真正获得前台焦点，导致游戏内部保持“隐藏光标”状态。
- H2: `ShowCursor` 的全局显示计数在启动阶段被错误减到了负数，而且进入游戏后没有再次恢复。
- H3: 游戏窗口出现与可交互之间存在时序差，当前处理点过早，导致前台切换或光标恢复没有落在有效时机。
- H4: 启动器保留可见时，某个启动后 UI 操作再次抢回焦点，覆盖了前面切到游戏窗口的动作。
- H5: 问题并非前台焦点，而是游戏窗口初始化/激活时需要再次显式恢复系统光标。

## Plan
- 仅添加调试插桩，记录 Minecraft 窗口出现、前台窗口变化、启动结束处理、光标计数恢复调用。
- 让用户复现一次并提供日志。
- 根据证据确认根因后，再做最小修复。

## Evidence
- `trae-debug-log-game-cursor-hidden.ndjson:3`：发现 Minecraft 窗口时，前台仍是启动器，不是游戏窗口。
- `trae-debug-log-game-cursor-hidden.ndjson:4-7`：启动结束处理和 `EnsureCursorVisible()` 发生时，前台依然是启动器。
- `trae-debug-log-game-cursor-hidden.ndjson:8-9`：约 220ms 后前台成功切换到 Minecraft，说明“切前台失败”不是主因。
- `trae-debug-log-game-cursor-hidden.ndjson:5-6`：启动结束时光标恢复确实执行了，但执行时机早于游戏真正接管前台。
- `trae-debug-log-game-cursor-hidden.ndjson:10`：启动器再次激活发生在更晚阶段，暂不构成启动瞬间主因。

## Verification
| ID | Hypothesis | Status | Evidence Summary |
|----|------------|--------|------------------|
| A | Minecraft 没拿到前台焦点 | ❌ Rejected | 日志 `8-9` 显示焦点切换成功，前台最终变成游戏窗口 |
| B | `ShowCursor` 计数始终错误 | ❌ Rejected | 日志 `5-6` 显示启动时恢复后计数为 `1`，恢复调用本身生效 |
| C | 处理时机过早 | ✅ Confirmed | 日志 `4-9` 显示光标恢复早于游戏真正成为前台窗口 |
| D | 启动器立即又抢回焦点 | ⏳ Inconclusive | 有激活日志，但发生在更晚时刻，不是启动瞬间主因 |
| E | 需要在游戏激活后再次恢复光标 | ✅ Confirmed | 结合 `4-9`，恢复点必须后移到游戏窗口真正激活之后 |

## Iteration 2
- 用户补充：问题出现在标题、暂停、背包这些本应显示系统光标的界面。
- 用户补充：`Alt+Tab` 切出去再切回游戏不会恢复。
- 用户补充：临时禁用 Java Launch Wrapper 后仍异常。
- 新假设 F：游戏启动发生在启动按钮的鼠标事件链尚未完全结束时，老版本在这一瞬间初始化出错误的光标状态。
