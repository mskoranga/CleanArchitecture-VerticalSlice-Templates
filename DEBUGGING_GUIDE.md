# Debugging Guide - Breakpoint Not Hitting Issue

## ✅ Solution Already Applied: Clean + Rebuild (COMPLETED)

The solution has been cleaned and rebuilt successfully. Now follow these steps:

---

## 🔍 **Troubleshooting Steps**

### Step 1: Verify Debug Configuration

1. **Check your build configuration:**
   - Open Visual Studio
   - Look at the top toolbar
   - Ensure it says **"Debug"** (not "Release")
   - Click the dropdown next to the play button and select **"Debug"** if needed

2. **Verify the project is set to debug mode:**
   - Right-click on `WebApi` project → Properties
   - Go to **Build** → **General**
   - Ensure "Optimize code" is **UNCHECKED** for Debug configuration

---

### Step 2: Set Breakpoints Correctly

For the **ProduceSchemalessMessageValidator.cs** file:

**Best places to set breakpoints:**

```csharp
// Line 7 - Constructor (will hit when validator is created)
public ProduceSchemalessMessageValidator()  // ← SET BREAKPOINT HERE
{
    RuleFor(x => x.Topic)  // ← OR HERE
        .NotEmpty().WithMessage("Topic is required")
```

**Note:** FluentValidation validators are created by DI and validated through decorators, so:
- The **constructor breakpoint (line 7)** will hit when the validator is instantiated
- The **rule definitions (lines 9-20)** won't hit during validation - they're just configuration
- To debug actual validation, you need to set breakpoints in the **Handler** or **Endpoint**

---

### Step 3: Set Breakpoints in the Right Places

Since validators run through the `ValidationDecorator`, set breakpoints here instead:

#### ✅ **ProduceSchemalessMessageHandler.cs**
```csharp
public async Task<Result<ProduceSchemalessMessageResponse>> HandleAsync(
    ProduceSchemalessMessageRequest request,
    CancellationToken cancellationToken)
{
    try  // ← SET BREAKPOINT HERE (Line ~21)
    {
        var messageId = Guid.CreateVersion7().ToString();  // ← OR HERE
```

#### ✅ **ProduceSchemalessMessageEndpoint.cs**
```csharp
app.MapPost("kafka/produce/schemaless", async (
    IHandler<ProduceSchemalessMessageRequest, Result<ProduceSchemalessMessageResponse>> handler,
    ProduceSchemalessMessageRequest request,
    CancellationToken cancellationToken) =>  // ← SET BREAKPOINT HERE (Line ~14)
{
    var result = await handler.HandleAsync(request, cancellationToken);  // ← OR HERE
```

---

### Step 4: Check Visual Studio Debugger Settings

1. **Tools** → **Options** → **Debugging** → **General**

2. **Ensure these are CHECKED:**
   - ✅ Enable Just My Code (unless you need to debug framework code)
   - ✅ Enable .NET Framework source stepping (optional)
   - ✅ Enable source server support

3. **Ensure these are UNCHECKED:**
   - ❌ Suppress JIT optimization on module load (should be unchecked)
   - ❌ Enable Edit and Continue (can cause issues)

4. **Go to:** **Debugging** → **Symbols**
   - Ensure "Microsoft Symbol Servers" is checked
   - Click "Load all symbols" if needed

---

### Step 5: Start Debugging Correctly

#### Method 1: Debug with F5
```powershell
# In Visual Studio:
1. Press F5 (or click the green "Play" button)
2. Wait for the application to start
3. Make an API call to trigger the endpoint
```

#### Method 2: Attach to Process
```powershell
# If already running:
1. Debug → Attach to Process (Ctrl+Alt+P)
2. Find "Apis.exe" or "dotnet.exe"
3. Click "Attach"
```

---

### Step 6: Test the Endpoint

Once the debugger is running, test the endpoint:

#### Using cURL:
```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "test-topic",
    "key": "test-key",
    "payload": {
      "test": "value"
    }
  }'
```

#### Using PowerShell:
```powershell
$body = @{
    topic = "test-topic"
    key = "test-key"
    payload = @{
        test = "value"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/kafka/produce/schemaless" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

#### Using Postman:
1. Import the `Postman_Collection.json` from the project
2. Run the "Produce Schemaless Message" request

---

### Step 7: Verify Application is Running

Check if the application started correctly:

```powershell
# Check if the port is listening
netstat -ano | findstr :5000

# Or check the Output window in Visual Studio
# Look for: "Now listening on: http://localhost:5000"
```

---

## 🎯 **Where to Set Breakpoints for Different Scenarios**

### To Debug Request Reception:
```csharp
// ProduceSchemalessMessageEndpoint.cs - Line 14
app.MapPost("kafka/produce/schemaless", async (
    IHandler<ProduceSchemalessMessageRequest, Result<ProduceSchemalessMessageResponse>> handler,
    ProduceSchemalessMessageRequest request,  // ← BREAKPOINT HERE
    CancellationToken cancellationToken) =>
```

### To Debug Validation:
```csharp
// Look in: src\Application\Pipelines\ValidationDecorator.cs
// Set breakpoint in the Validate method
```

### To Debug Handler Logic:
```csharp
// ProduceSchemalessMessageHandler.cs - Line 21
public async Task<Result<ProduceSchemalessMessageResponse>> HandleAsync(
    ProduceSchemalessMessageRequest request,
    CancellationToken cancellationToken)
{
    try  // ← BREAKPOINT HERE
    {
        var messageId = Guid.CreateVersion7().ToString();
```

### To Debug Kafka Producer:
```csharp
// Infrastructure/Messaging/Kafka/Producers/KafkaProducer.cs
public async Task ProduceAsync<TMessage>(string topic, TMessage message, ...)
{
    try  // ← BREAKPOINT HERE
    {
```

---

## 🔧 **Advanced Troubleshooting**

### Issue: Breakpoint is Hollow (White/Empty Circle)

**Causes:**
1. ❌ Code is optimized (Release mode)
2. ❌ PDB files don't match the DLL
3. ❌ Code hasn't been loaded yet

**Solutions:**
```powershell
# 1. Clean bin/obj folders manually
Remove-Item -Path "src\*/bin" -Recurse -Force
Remove-Item -Path "src\*/obj" -Recurse -Force
Remove-Item -Path "tests\*/bin" -Recurse -Force
Remove-Item -Path "tests\*/obj" -Recurse -Force

# 2. Rebuild
dotnet build

# 3. Start debugging again
```

### Issue: Breakpoint Shows Warning Symbol

**Message:** "The breakpoint will not currently be hit. No symbols have been loaded for this document."

**Solution:**
1. While debugging, go to **Debug** → **Windows** → **Modules**
2. Find your assembly (e.g., `Application.dll`)
3. Right-click → **Load Symbols**
4. If "Symbol file not found", right-click → **Symbol Settings** → Add symbol path

### Issue: Code Doesn't Match Breakpoint

**Solution:**
1. Stop debugging
2. Delete all `bin` and `obj` folders
3. Rebuild solution
4. Restart Visual Studio
5. Start debugging again

---

## 📋 **Quick Checklist**

Before debugging, verify:

- ✅ Solution is in **Debug** configuration (not Release)
- ✅ "Optimize code" is **UNCHECKED** in project properties
- ✅ Solution has been **cleaned and rebuilt** (DONE ✅)
- ✅ Breakpoint is on an **executable line** (not a comment or declaration)
- ✅ Application is **running with debugger attached** (F5)
- ✅ You're making a **request to the endpoint** (API call)
- ✅ No errors in the **Output window** or **Error List**

---

## 🎬 **Step-by-Step Debugging Session**

1. **Set breakpoint** in `ProduceSchemalessMessageHandler.cs` line 21 (the `try` statement)

2. **Press F5** to start debugging

3. **Wait** for Visual Studio to show: "Now listening on: http://localhost:5000"

4. **Send request** using cURL/Postman/PowerShell:
   ```bash
   curl -X POST http://localhost:5000/kafka/produce/schemaless \
     -H "Content-Type: application/json" \
     -d '{"topic":"test","key":"key1","payload":{"test":"value"}}'
   ```

5. **Breakpoint should hit!** You'll see:
   - Yellow arrow on the breakpoint line
   - Variables window populated with values
   - Call stack showing the execution path

6. **Use debugger controls:**
   - **F10** - Step Over (execute current line)
   - **F11** - Step Into (go into method calls)
   - **Shift+F11** - Step Out (exit current method)
   - **F5** - Continue execution

---

## 🆘 **Still Not Working?**

### Try These Steps:

1. **Restart Visual Studio**
   ```powershell
   # Close Visual Studio completely
   # Reopen the solution
   ```

2. **Check for multiple instances**
   ```powershell
   # Make sure only ONE instance of the app is running
   Get-Process | Where-Object {$_.ProcessName -like "*Apis*"}
   ```

3. **Delete .vs folder**
   ```powershell
   # Close Visual Studio first!
   Remove-Item -Path ".vs" -Recurse -Force
   # Reopen Visual Studio
   ```

4. **Check launchSettings.json**
   ```json
   // src/WebApi/Properties/launchSettings.json
   {
     "profiles": {
       "Apis": {
         "commandName": "Project",
         "launchBrowser": true,
         "applicationUrl": "http://localhost:5000",
         "environmentVariables": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         }
       }
     }
   }
   ```

5. **Verify the endpoint is registered**
   - Check the Output window for: "Endpoint registered: POST kafka/produce/schemaless"
   - If not found, the DI registration might have an issue

---

## 💡 **Pro Tips**

1. **Use Conditional Breakpoints:**
   - Right-click breakpoint → Conditions
   - Example: `request.Topic == "test-topic"`

2. **Use Tracepoints:**
   - Right-click breakpoint → Actions
   - Log message without stopping execution

3. **Watch Window:**
   - Add variables to watch: `request.Topic`, `request.Payload`

4. **Immediate Window:**
   - Type expressions while debugging
   - Example: `request.Payload.Count`

---

## 📞 **Need More Help?**

If breakpoints still don't work after all these steps:

1. Check the **Output** window (View → Output)
2. Select "Debug" from the dropdown
3. Look for any error messages
4. Check **Error List** window for build warnings

---

**Remember:** The solution has already been **cleaned and rebuilt successfully** ✅

Now just:
1. Set a breakpoint in the **Handler** (not the validator constructor)
2. Press **F5**
3. Send an API request
4. Breakpoint should hit! 🎯
