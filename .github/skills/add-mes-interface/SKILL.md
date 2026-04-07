---
name: add-mes-interface
description: "Use when adding a new MES, FMS, or ECS integration in this workspace, including request and response models, service-layer API methods, enable switches, MediatR command wrappers, handler wiring, and settings UI updates. Keywords: 添加mes接口, 添加fms接口, 添加ecs接口, 新增接口使能, api使能, 中介者命令, MediatR, handler封装."
---

# Add MES Interface

## Purpose

Use this skill when the task is to add a new MES, FMS, or ECS interface call in this repository pattern, not just to change an ASP.NET controller route.

This workspace commonly implements external system integrations through these layers:

- Request and response models in service code
- Service methods in classes such as `NewFMSService`, `NewECSService`, or `MesService`
- MediatR command or query records under `Mediator/ECS` or related mediator folders
- Handler implementations under `Mediator/Handlers/ECS`
- Runtime enable switches in global save config or station-level config
- WPF settings UI in pages such as `SettingView.xaml` or station dialogs such as `StationSettingView.xaml`

## When To Use

Use this skill for requests like:

- 添加一个新的 MES 接口调用
- 给 FMS 接口增加请求模型和 handler 封装
- 新增接口使能开关
- 把接口包装成中介者命令
- 在托盘到位、退盘、抓取开始或结束等流程里接入新接口

Do not use this skill for pure ASP.NET route changes. For controller or `MapPost` contract changes, prefer `modify-api-interface`.

## What To Do

1. Confirm the integration contract.

- Capture the exact path, HTTP method, request JSON, response shape, required defaults, and triggering business event.
- Confirm whether the interface is MES, FMS, or ECS, because the repository uses different service wrappers for each.

2. Find the real integration layer before editing.

- Search `Service/` for existing similar methods, especially `NewFMSService`, `NewECSService`, and `MesService`.
- Search `Mediator/ECS` and `Mediator/Handlers/ECS` for matching command naming patterns.
- Search the relevant task or workflow handler such as pallet arrived, pallet out, grab action, or material apply.

3. Add request and response models in the existing style.

- Reuse the current serialization conventions such as `JsonPropertyName` where already used.
- Keep field names aligned with the actual external API contract.
- Do not infer extra fields unless the user or the existing integration contract requires them.

4. Add the service-layer method.

- For FMS integrations, prefer adding the method to `NewFMSService` and route it through `SendFmsRequest<T>`.
- For ECS integrations, use the existing ECS service abstraction rather than bypassing it.
- For MES integrations, follow the existing `MesService` patterns already used in the workspace.
- Preserve existing logging, retry, and warning behavior patterns unless the user asks for a different policy.

5. Wrap the call in a MediatR command or query.

- Add a record under the existing mediator folder, usually `Mediator/ECS` for machine workflow integrations.
- The command should carry the real runtime context needed by the handler, such as `Key`, `InsideStationNum`, tray code, or channel data.
- Avoid forcing handlers to rediscover context that the caller already knows.

6. Implement the handler at the business boundary.

- Put the handler under `Mediator/Handlers/ECS` when it belongs to a machine workflow.
- Validate required runtime data before calling the external interface.
- Use the existing warning command pattern, such as `WcsDataWarningBoxCommand`, when the codebase already uses it for operator-visible failures.
- Keep failure messages specific enough for现场排查.

7. Add the enable switch in the correct scope.

- Use `ApiEnableTable` in `WcsSaves` only when the switch is truly global for a stage.
- Use station-level fields such as `XYLStation` or `Pallet` when the interface should be enabled per station.
- Choose scope based on where the trigger lives, not based on what is easiest to implement.

8. Expose the switch in the correct WPF configuration screen.

- Global switches usually belong in `View/SettingView.xaml`.
- Station-specific switches usually belong in `View/CZPallet/StationSettingView.xaml` and its code-behind.
- Keep labels explicit about what event triggers the interface, for example whether it is used on pallet out, pallet arrived, or group-end upload.

9. Wire the handler into the real workflow.

- Update the actual command handler or PLC task flow that should trigger the new interface.
- Prefer calling the mediator command from the existing workflow handler rather than calling the service directly from UI or task code.
- If the interface depends on pallet or station state, resolve that state once and pass the required identifiers into the mediator command.

10. Verify the full slice.

- Check errors in every edited file.
- Run a targeted build if feasible.
- Confirm the enable switch path, handler path, and workflow call site are all connected.

## Repository Patterns To Expect

In this workspace, common patterns include:

- External request and response DTOs defined near service implementations
- `NewFMSService` methods using `SendFmsRequest<T>`
- MediatR records under `Mediator/ECS`
- Handler implementations under `Mediator/Handlers/ECS`
- Runtime warnings emitted through `WcsDataWarningBoxCommand`
- Stage-global config in `WcsSaves.ApiEnableTable`
- Station-specific config in models such as `XYLStation` and `Pallet`
- WPF settings maintained in code-behind heavy dialogs rather than pure MVVM views

When adding a new interface, inspect all of these layers before deciding where the change belongs.

## Guardrails

- Do not only add the service method and stop there; complete the trigger path end to end.
- Do not add a global enable switch when the behavior is clearly station-specific.
- Do not add a station-specific switch when the integration is truly stage-global.
- Do not invent request fields or default values that are not supported by the external contract.
- Preserve the repository's naming and error-reporting conventions.
- Prefer root-cause integration changes over UI-only toggles or direct service calls from unrelated layers.

## Typical Outputs

A complete change usually includes most of these:

- A new request model
- A new response model if needed
- A new service method
- A new MediatR command or query record
- A new handler implementation
- A new enable switch in the correct config scope
- A settings UI control for that switch
- A workflow call site that actually triggers the command

## Completion Checklist

- Confirmed the external API contract
- Found the correct service layer for the interface
- Added or updated request and response models
- Added the service method using existing infrastructure
- Added a MediatR command or query wrapper
- Implemented the handler with validation and operator-visible failure behavior where appropriate
- Added the enable switch in the right scope
- Exposed the switch in the right configuration UI
- Wired the command into the intended workflow
- Checked edited files for errors and, when feasible, ran a build