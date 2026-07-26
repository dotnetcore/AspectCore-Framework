# 3.0.0 GA 发布流程

本文说明如何把 AspectCore 从 `3.0.0-rc.1` 正式发布为 `3.0.0` GA。发布由 `.github/workflows/release.yml` 在收到 `v*` tag（或手动 `workflow_dispatch`）后自动执行:先过质量门禁,再打包、推 NuGet/MyGet、建 GitHub Release,最后回写版本号。所有命令与机制均以仓库中 `.github/workflows/release.yml`、`.github/scripts/check-coverage.sh`、`build/version.props` 的实际实现为准。

> ⚠️ 发布是不可逆的对外操作(NuGet 包一旦发布无法删除,只能 unlist)。请务必先走完「发布前检查清单」,确认无误后再打 tag。

---

## 1. 发布机制总览

release.yml 的 job 依赖链如下(前者全绿,后者才运行):

```
lint ┐
unit-coverage ┐
e2e-coverage ├─► quality-gate ─► build-and-test ─► update-version
nativeaot-coverage ┤              (打包 + 推 NuGet/MyGet     (回写版本号 +
nativeaot-verify ┤               + 建 GitHub Release)        开 version bump PR)
codeql ┘
```

关键点:

- **质量门禁前置且强制**。`quality-gate` 通过 `needs:` 依赖全部 6 个门禁 job,且不使用 `if: always()`。只要任一门禁失败,`quality-gate` 会被跳过 → `build-and-test` 随之被跳过 → **不会 push NuGet/MyGet,也不会创建 GitHub Release**。
- **门禁与 PR CI 同源**。release 门禁复用与 `build-pr-ci.yml` 完全相同的 `.github/scripts/check-coverage.sh`,覆盖率阈值硬编码在脚本内,两条流水线共用,不会漂移:
  - 单元测试覆盖率 ≥ **95%**
  - E2E 覆盖率 ≥ **80%**
  - NativeAOT 单测覆盖率 ≥ **100%**
  - NativeAOT E2E 覆盖率 ≥ **95%**
  - 另有 lint(`dotnet format --verify-no-changes`)、CodeQL 安全分析、NativeAOT `publish` + 运行原生二进制。
- **VersionQuality 由 GA tag 自动清空**,无需手工提交。`build/version.props` 当前为 `<VersionQuality>rc.1</VersionQuality>`。发布成功后,`update-version` job 会在**稳定版 tag**(tag 名不含 `-`)时把 `VersionQuality` 置空并开一个 version bump PR。因此**不要手动提交清空 VersionQuality 的改动**——那是 tag 触发后由流水线自动完成的。

---

## 2. 发布前检查清单

打 tag 之前逐项确认:

- [ ] **PR CI 全绿**:待发布的 master HEAD 对应的最后一个 PR,`build-pr-ci.yml` 的全部 9 个 job(lint / 双 OS build / 单测覆盖率 / E2E 覆盖率 / NativeAOT 覆盖率 / CodeQL)均通过。
- [ ] **NativeAOT publish + run 通过**:`nativeaot-verify.yml` 在 master 上绿(publish 出的 `linux-x64` 原生二进制能正常运行退出)。
- [ ] **覆盖率达标**:单测 ≥ 95%、E2E ≥ 80%、NativeAOT 单测 = 100%、NativeAOT E2E ≥ 95%(与门禁一致)。
- [ ] **发布说明就绪**:`docs/release-notes/v3.0.0.md` 内容已定稿,GitHub Release 采用 `generate_release_notes: true` 自动汇总 PR,可与该文档互补。
- [ ] **版本号正确**:`build/version.props` 为 `3 / 0 / 0`,`VersionQuality` 仍为 `rc.1`(**保持不动**,由 tag 自动清空)。
- [ ] **本地干净**:准备打 tag 的 commit 就是要发布的 master HEAD,无未合并的关键改动。

> 快速自检覆盖率(可选,在本地或 CI 上运行,与门禁同脚本):
>
> ```bash
> ./.github/scripts/check-coverage.sh collect unit --output /tmp/unit.env
> ./.github/scripts/check-coverage.sh assert  unit --input  /tmp/unit.env
> ```

---

## 3. 执行发布(确切命令序列)

确认清单全绿后,在 master HEAD 上执行:

```bash
# 1. 确认当前在 master 且是要发布的提交
git checkout master
git pull --ff-only origin master

# 2. 打 GA tag —— 注意:tag 名不带任何 "-" 后缀
git tag v3.0.0

# 3. 推送 tag,触发 release.yml
git push origin v3.0.0
```

`git push origin v3.0.0` 会触发 `release.yml`,自动完成:

1. 跑质量门禁(lint / coverage / CodeQL / NativeAOT publish+run);全绿后
2. `build-and-test`:按 `FULL_VERSION=3.0.0`(从 tag 去掉 `v` 前缀得到)编译、`dotnet pack`、校验包数量;
3. 推包到 **NuGet.org**(OIDC 换取临时 API key,带重试)与 **MyGet**;
4. 用 `softprops/action-gh-release` 创建 **GitHub Release**(附 `.nupkg`/`.snupkg`,自动生成 release notes);
5. `update-version`:因 tag `v3.0.0` **不含 `-`**,识别为稳定版 → 把 `build/version.props` 的 `VersionQuality` 置空、版本 bump 到下一个 minor(`3.1.0`),并开一个 version bump PR 供合并。

### ⚠️ tag 命名硬性要求:不能带 `-`

release.yml 的 `update-version` job 用 `!contains(github.ref_name, '-')` 判断是否为稳定版:

- `v3.0.0` ✅ —— 稳定版,发布后自动清空 `VersionQuality`。
- `v3.0.0-rc.2`、`v3.0.0-preview1` ❌ —— 含 `-`,被识别为**预发布**,`update-version` 不运行,**不会清空 VersionQuality**,只做打包发布。

因此 GA 发布**必须**用 `v3.0.0` 这种不含 `-` 的 tag,否则版本后缀不会被清空。

---

## 4. 发布后验证

- 访问 GitHub Actions,确认 `Release` workflow 全部 job 成功(尤其 `quality-gate` → `build-and-test` → `update-version`)。
- 在 [NuGet.org](https://www.nuget.org/) 搜索 `AspectCore.*`,确认 `3.0.0` 版本已上架(NuGet 索引可能有几分钟延迟)。
- 确认 GitHub Releases 页面出现 `v3.0.0`。
- Review 并合并 `update-version` 自动开的 version bump PR(把仓库基线推进到下一个开发版本)。

---

## 5. 回滚 / 应急

发布是对外不可逆操作,出问题时按以下顺序处理:

- **门禁未过 → 发布被自动拦截**:这是预期行为,不会有包被推出。修复问题后重新走流程即可。若同名 tag 已存在但发布被拦截,可先删本地/远端 tag 再重打:
  ```bash
  git tag -d v3.0.0
  git push origin :refs/tags/v3.0.0   # 删除远端 tag
  ```
  > 删除远端 tag 属于对外动作,确认无正在进行的发布后再执行。
- **包已推到 NuGet 但发现问题**:NuGet.org 的包**不能删除**,只能 **unlist**(隐藏,不影响已依赖它的用户),然后发布修订版本 `3.0.1`。unlist 在 NuGet.org 包管理页操作。
- **GitHub Release 有误**:可在 Releases 页面编辑或删除该 Release(不影响已发布的 NuGet 包)。
- **紧急修复走 patch 版本**:按 `build/version.props` 注释说明,patch 版本(如 `3.0.1`)需手动更新 `build/version.props`,再打 `v3.0.1` tag 走同一发布流程。

---

## 相关文件

- 发布流水线:`.github/workflows/release.yml`
- 覆盖率门禁脚本(阈值来源):`.github/scripts/check-coverage.sh`
- PR CI(门禁同源参照):`.github/workflows/build-pr-ci.yml`
- NativeAOT 验证:`.github/workflows/nativeaot-verify.yml`
- 版本定义:`build/version.props`
- 发布说明:`docs/release-notes/v3.0.0.md`
