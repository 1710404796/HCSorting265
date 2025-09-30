# NewMesService ECS接口实现文档

## 概述

`NewMesService` 是基于现有 `MesService` 架构实现的ECS接口服务类，提供了完整的ECS设备通用接口实现。该服务遵循相同的错误处理、日志记录和重试机制。

## 功能特性

### 🔧 核心功能
- **设备进站接口** - 设备进站验证，调用ECS进站接口
- **设备出站接口** - 设备出站验证，调用ECS出站接口  
- **设备状态上报** - 设备状态变化或异常时上报
- **库位状态上报** - 库位状态发生变化时上报
- **火警信息上报** - 设备火警上报，由ECS转发给FMS

### 🛠️ 技术特性
- **统一错误处理** - 继承MesService的错误处理机制
- **自动重试机制** - 失败时提供用户选择重试或取消
- **完整日志记录** - 记录所有请求/响应到WcsLog表
- **强类型模型** - 完整的请求/响应数据模型
- **异步操作** - 全异步接口设计

## 接口详情

### 1. 设备进站接口

**方法：** `EquipEnterAsync(EquipEnterRequest request)`

**请求参数：**
```csharp
public class EquipEnterRequest
{
    public string EquipNum { get; set; }     // 设备号（必选）
    public int EquipType { get; set; }       // 设备类型（必选）
    public string TrayCode { get; set; }     // 托盘码（必选）
    public string? Location { get; set; }    // 位置（可选）
    public string? TrayType { get; set; }    // 托盘类型（可选）
    public string? CellCode { get; set; }    // 电芯码（可选）
}
```

**响应结果：**
```csharp
public class EquipEnterResponse : EcsResponseBase
{
    public EquipEnterResult? Result { get; set; }
}

public class EquipEnterResult
{
    public int ChannelNum { get; set; }      // 通道数量
    public CellInfo[] CellList { get; set; } // 电芯列表
    public string? TrayCode { get; set; }    // 托盘码
    public int TrayType { get; set; }        // 托盘类型
    public int Ret { get; set; }             // 返回码
    public string[] NgCodes { get; set; }    // NG码数组
}
```

### 2. 设备出站接口

**方法：** `EquipOutAsync(EquipOutRequest request)`

**参数与进站接口相同，响应结构相同**

### 3. 设备状态上报

**方法：** `EquipStatusReportAsync(EquipStatusRequest[] request)`

**请求参数：**
```csharp
public class EquipStatusRequest
{
    public string EquipNum { get; set; }     // 设备号（必选）
    public int EquipType { get; set; }       // 设备类型（必选）
    public string? Location { get; set; }    // 位置（可选）
    public string Status { get; set; }       // 设备状态（必选）
    public string? TrayCode { get; set; }    // 托盘码（可选）
    public string? CellCode { get; set; }    // 电芯码（可选）
    public string? ErrorMsg { get; set; }    // 错误信息（可选）
    public string? ErrorCode { get; set; }   // 错误码（可选）
}
```

### 4. 库位状态上报

**方法：** `StageStatusReportAsync(StageStatusRequest[] request)`

**参数结构与设备状态上报相同**

### 5. 火警信息上报

**方法：** `FireAlarmReportAsync(FireAlarmRequest request)`

**请求参数：**
```csharp
public class FireAlarmRequest
{
    public string EquipNum { get; set; }      // 设备号（必选）
    public int EquipType { get; set; }        // 设备类型（必选）
    public string WareHouseName { get; set; } // 库位名称（必选）
    public string StackerNum { get; set; }    // 堆垛机编号（必选，长度3）
    public string Location { get; set; }      // 库位号（必选，长度9）
    public string LocationType { get; set; }  // 深浅位（必选，0无，1深，2浅）
}
```

## 设备类型枚举

```csharp
public enum EquipType
{
    OCV = 0,                // OCV
    化成_FMT = 1,           // 化成 FMT
    DCIR = 2,               // DCIR
    筛选_SEL = 3,           // 筛选 SEL
    物流线_CV = 4,          // 物流线 CV
    堆垛机_STACKER = 5,     // 堆垛机 STACKER
    扫码枪_BCR = 6,         // 扫码枪 BCR
    组盘机_AEB = 7,         // 组盘机 AEB
    拆盘机_DAEB = 8,        // 拆盘机 DAEB
    其它_OTHER = 24         // 其它 OTHER
}
```

## 设备状态枚举

```csharp
public enum EquipStatus
{
    I,  // 空闲 (Idle)
    T,  // 有托盘 (Tray)
    R,  // 运行 (Running)
    E,  // 结束 (End)
    D,  // 禁用 (Disabled)
    F   // 故障 (Fault)
}
```

## 使用示例

### 基本用法

```csharp
// 创建服务实例
var newMesService = new NewMesService(httpClient, "WorkStation1", logger);

// 设备进站
var enterRequest = new EquipEnterRequest
{
    EquipNum = "EQ001",
    EquipType = (int)EquipType.OCV,
    TrayCode = "TRAY123456",
    Location = "LOC001"
};

var enterResult = await newMesService.EquipEnterAsync(enterRequest);

// 处理结果
enterResult.Match(
    success => 
    {
        // 进站成功处理
        Console.WriteLine($"通道数: {success.Result?.ChannelNum}");
        return true;
    },
    failure =>
    {
        // 失败处理
        Console.WriteLine($"进站失败: {failure.Message}");
        return false;
    }
);
```

### 设备状态上报

```csharp
// 设备运行状态上报
var statusRequests = new EquipStatusRequest[]
{
    new EquipStatusRequest
    {
        EquipNum = "EQ001",
        EquipType = (int)EquipType.OCV,
        Status = EquipStatus.R.ToString(), // 运行状态
        Location = "LOC001",
        TrayCode = "TRAY123456"
    }
};

var statusResult = await newMesService.EquipStatusReportAsync(statusRequests);
```

### 设备故障上报

```csharp
// 设备故障状态上报
var errorRequests = new EquipStatusRequest[]
{
    new EquipStatusRequest
    {
        EquipNum = "EQ001",
        EquipType = (int)EquipType.OCV,
        Status = EquipStatus.F.ToString(), // 故障状态
        ErrorCode = "E001",
        ErrorMsg = "传感器故障"
    }
};

var errorResult = await newMesService.EquipStatusReportAsync(errorRequests);
```

### 调度异常上报

```csharp
// 调度异常上报（ErrorCode=99）
var scheduleErrorRequests = new EquipStatusRequest[]
{
    new EquipStatusRequest
    {
        EquipNum = "EQ001",
        EquipType = (int)EquipType.OCV,
        Status = EquipStatus.F.ToString(),
        ErrorCode = "99", // 调度异常
        ErrorMsg = "调度系统异常"
    }
};

var result = await newMesService.EquipStatusReportAsync(scheduleErrorRequests);
```

## 配置要求

### appsettings.json 配置
确保在 `WcsSaves` 配置中包含 ECS 服务器地址：

```json
{
  "KeysSave": {
    "WorkStation1": {
      "ECSIP": "192.168.1.100:8080",
      // ... 其他配置
    }
  }
}
```

### 依赖注入配置

```csharp
// 在 Startup.cs 或 Program.cs 中注册服务
services.AddScoped<NewMesService>(provider => 
    new NewMesService(
        provider.GetRequiredService<HttpClient>(), 
        "WorkStation1", 
        provider.GetRequiredService<ILogger<NewMesService>>()
    )
);
```

## 重要注意事项

### 🚨 关键点
1. **筛选设备特殊处理** - 筛选设备的 `location` 字段需传输 NG 托盘、筛选托盘
2. **错误码规范** - `ErrorCode=99` 表示调度异常，其他为设备错误码
3. **火警上报字段长度** - 堆垛机编号长度为3，库位号长度为9
4. **状态上报数组** - 设备状态和库位状态上报支持批量操作

### 🔄 错误处理流程
1. 请求失败时自动记录日志到 `WcsLog` 表
2. 弹出确认对话框询问用户是否重试
3. 用户选择 OK 时自动重试，选择 Cancel 时返回失败结果
4. 所有异常都会被捕获并转换为 `EcsException`

### 📊 日志记录
- 记录完整的请求/响应数据
- 包含时间戳、API名称、成功状态
- 便于问题追踪和性能分析

## 扩展性

该实现可以轻松扩展以支持更多 ECS 接口，只需：

1. 添加新的请求/响应模型类
2. 在 `NewMesService` 中添加对应的方法
3. 调用 `SendEcsRequest<T>()` 方法处理请求

遵循现有的模式可以确保一致的错误处理和日志记录机制。
