---
name: modify-api-interface
description: "Use when modifying API endpoints, changing request or response models, adding API parameters, adjusting controller actions, updating MapGet or MapPost handlers, or refactoring ASP.NET Core interfaces. Keywords: 修改api接口, 修改接口, 新增接口字段, 调整返回值, 修改请求参数, controller api, MapGet, MapPost, HttpGet, HttpPost."
---

# Modify API Interface

## Purpose

Use this skill when the task is to modify an existing API interface or add related API behavior in an ASP.NET Core codebase.

This workflow is designed for repositories that may define APIs in either of these styles:

- Controller-based APIs with `ApiController`, `Route`, `HttpGet`, and `HttpPost`
- Endpoint routing in startup code with `MapGet`, `MapPost`, and related route mappings

## What To Do

1. Confirm the requested API change.

- Clarify whether the user wants to change route, HTTP method, request parameters, body schema, response schema, validation, side effects, or error handling.
- If the request is ambiguous, ask for the exact endpoint path, caller expectations, and compatibility constraints.

2. Find the real API entry point.

- Search for route declarations such as `HttpGet`, `HttpPost`, `Route`, `MapGet`, `MapPost`, `MapPut`, and `MapDelete`.
- Do not assume all APIs live under `Controllers`; some projects wire endpoints in `Startup.cs` or `Program.cs`.

3. Trace the impact before editing.

- Identify request and response DTOs, command objects, query objects, MediatR handlers, services, repositories, and validators used by the endpoint.
- Check for serialization attributes, `FromBody`, `FromQuery`, `FromRoute`, and nullable handling.
- Inspect downstream callers if the change may break compatibility.

4. Implement the change at the correct layer.

- Keep the public contract and internal behavior aligned.
- If request or response shapes change, update all related models and bindings.
- If business behavior changes, modify the underlying handler or service rather than only patching the controller.
- Preserve the existing project style and naming conventions.

5. Verify the API behavior.

- Check for compile errors in edited files.
- If feasible, run targeted build or tests for the affected project.
- Verify route template, binding source, nullability, and returned status codes.

6. Summarize the change clearly.

- State which endpoint changed.
- State whether the contract changed.
- Call out any compatibility risk for existing callers.

## Repository Patterns To Expect

Common patterns in this workspace include:

- `Startup.cs` endpoint mapping, for example `routes.MapGet(...)` and `routes.MapPost(...)`
- MVC controllers under `Controllers` or `Apis`
- MediatR-based command and query dispatch from controller actions

When editing, inspect both direct endpoint code and any downstream handlers before concluding the change is complete.

## Guardrails

- Prefer root-cause changes instead of controller-only workarounds.
- Avoid changing unrelated endpoints in the same file.
- Preserve backward compatibility unless the user explicitly asks for a breaking change.
- If a breaking change is necessary, note it explicitly in the final response.
- Do not invent request fields, response fields, or route conventions that are not supported by the existing codebase.

## Typical Inputs

Examples of requests that should trigger this skill:

- 修改某个 API 的入参
- 给接口新增一个返回字段
- 把 `MapPost` 接口改成支持新的 body 结构
- 修改 Controller 的路由和返回值
- 调整接口调用的命令对象或 DTO

## Completion Checklist

- Located the endpoint definition
- Located all impacted DTO or command objects
- Implemented behavior at the correct layer
- Checked affected files for errors
- Reported contract changes and risks