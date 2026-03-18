# CLAUDE.md

## Ralph Loop 使用指南

### 正确的启动方式

```bash
/ralph-loop "你的提示词" \
  --max-iterations 100 \
  --completion-promise "FINAL_DONE"
```

### 关键规则

1. **不要提前输出完成标记**
   - 只有真正完成所有任务时才输出 `<promise>FULL_TASK_COMPLETE</promise>`
   - 如果一轮完成一个子任务，继续下一轮，不要输出完成标记

2. **确保使用正确的完成承诺**
   - 启动时设置的 `--completion-promise` 必须与输出的一致
   - 如果不确定，可以不设置 completion promise

3. **检查状态文件**
   ```bash
   cat .claude/ralph-loop.local.md
   ```

4. **手动停止**
   ```bash
   /cancel-ralph
   ```

### 持续开发的正确流程

```
第1轮: 开发任务A
  ↓ (不要输出完成标记，如果还有其他任务)
第2轮: 开发任务B
  ↓ (不要输出完成标记，如果还有其他任务)
第3轮: 开发任务C
  ↓ (所有任务完成后，输出完成标记)
结束
```

### 常见问题

**Q: 为什么只运行一轮就结束了？**
A: 可能原因：
1. 你输出了完成标记
2. max-iterations 设置太小
3. 状态文件被删除

**Q: 如何检查 loop 是否活跃？**
A: 运行：`cat .claude/ralph-loop.local.md`

**Q: 如何确保 loop 持续运行？**
A: 使用 `--max-iterations 100` 并且不要在每轮结束后输出完成标记。

---

## 项目特定提示

本项目使用 Ralph Loop 进行持续开发时：

1. 每轮专注于一个具体任务
2. 完成后 git commit
3. 更新 Roadmap
4. 继续下一轮
5. 所有任务完成后再输出完成标记
