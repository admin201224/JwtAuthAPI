#!/usr/bin/env pwsh

# Colors
$Green = [char]27 + '[32m'
$Yellow = [char]27 + '[33m'
$Red = [char]27 + '[31m'
$Reset = [char]27 + '[0m'

Write-Host "${Green}=== JWT API Test Script ===${Reset}" -ForegroundColor Green

# Check if API is running
Write-Host "${Yellow}Checking if API is running...${Reset}"
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger" -UseBasicParsing -ErrorAction Stop
    Write-Host "${Green}? API is running!${Reset}" -ForegroundColor Green
}
catch {
    Write-Host "${Red}? API is not running. Please run: dotnet run${Reset}" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 1: Login as Admin
Write-Host "${Yellow}Test 1: Login as Admin${Reset}"
$loginBody = @{
    username = "admin"
    password = "Admin@123456"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/authorize/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody `
        -UseBasicParsing

    $data = $response.Content | ConvertFrom-Json
    $accessToken = $data.accessToken

    if ($accessToken) {
        Write-Host "${Green}? Login successful!${Reset}" -ForegroundColor Green
        Write-Host "Access Token: $($accessToken.Substring(0, 50))..."
    }
    else {
        Write-Host "${Red}? Login failed: No token returned${Reset}" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "${Red}? Login failed: $($_.Exception.Message)${Reset}" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Get All Users (Admin)
Write-Host "${Yellow}Test 2: Get All Users (Admin)${Reset}"
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/user" `
        -Method GET `
        -Headers @{"Authorization" = "Bearer $accessToken"} `
        -UseBasicParsing

    $data = $response.Content | ConvertFrom-Json
    Write-Host "${Green}? Users retrieved successfully!${Reset}" -ForegroundColor Green
    Write-Host "Count: $($data.count)"
    Write-Host "Users:"
    $data.users | ForEach-Object {
        Write-Host "  - $($_.username) ($($_.role))"
    }
}
catch {
    Write-Host "${Red}? Failed to get users: $($_.Exception.Message)${Reset}" -ForegroundColor Red
}

Write-Host ""

# Test 3: Get Current Profile
Write-Host "${Yellow}Test 3: Get Current Profile${Reset}"
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/user/me/profile" `
        -Method GET `
        -Headers @{"Authorization" = "Bearer $accessToken"} `
        -UseBasicParsing

    $data = $response.Content | ConvertFrom-Json
    Write-Host "${Green}? Profile retrieved successfully!${Reset}" -ForegroundColor Green
    Write-Host "Username: $($data.user.username)"
    Write-Host "Role: $($data.user.role)"
}
catch {
    Write-Host "${Red}? Failed to get profile: $($_.Exception.Message)${Reset}" -ForegroundColor Red
}

Write-Host ""

# Test 4: Create New User (Admin)
Write-Host "${Yellow}Test 4: Create New User (Admin)${Reset}"
$createBody = @{
    username = "testuser$(Get-Random)"
    password = "Test@123456"
    role = "User"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/user" `
        -Method POST `
        -ContentType "application/json" `
        -Headers @{"Authorization" = "Bearer $accessToken"} `
        -Body $createBody `
        -UseBasicParsing

    $data = $response.Content | ConvertFrom-Json
    $newUserId = $data.user.id
    Write-Host "${Green}? User created successfully!${Reset}" -ForegroundColor Green
    Write-Host "New User ID: $newUserId"
    Write-Host "Username: $($data.user.username)"
}
catch {
    Write-Host "${Red}? Failed to create user: $($_.Exception.Message)${Reset}" -ForegroundColor Red
}

Write-Host ""

# Test 5: Login as User
Write-Host "${Yellow}Test 5: Login as Regular User${Reset}"
$loginBody = @{
    username = "user"
    password = "User@123456"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/authorize/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody `
        -UseBasicParsing

    $data = $response.Content | ConvertFrom-Json
    $userToken = $data.accessToken

    if ($userToken) {
        Write-Host "${Green}? User login successful!${Reset}" -ForegroundColor Green
    }
}
catch {
    Write-Host "${Red}? User login failed: $($_.Exception.Message)${Reset}" -ForegroundColor Red
}

Write-Host ""

# Test 6: User Try to Get All Users (Should Fail)
Write-Host "${Yellow}Test 6: User Try to Get All Users (Should Fail - 403)${Reset}"
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/user" `
        -Method GET `
        -Headers @{"Authorization" = "Bearer $userToken"} `
        -UseBasicParsing -ErrorAction SilentlyContinue
}
catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        Write-Host "${Green}? Correctly rejected (403 Forbidden)${Reset}" -ForegroundColor Green
    }
    else {
        Write-Host "${Yellow}? Unexpected error: $($_.Exception.Response.StatusCode)${Reset}" -ForegroundColor Yellow
    }
}

Write-Host ""

# Test 7: Logout
Write-Host "${Yellow}Test 7: Logout (Revoke Token)${Reset}"
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/authorize/revoke" `
        -Method POST `
        -Headers @{"Authorization" = "Bearer $accessToken"} `
        -UseBasicParsing

    $data = $response.Content | ConvertFrom-Json
    Write-Host "${Green}? Logout successful!${Reset}" -ForegroundColor Green
    Write-Host "Message: $($data.message)"
}
catch {
    Write-Host "${Red}? Logout failed: $($_.Exception.Message)${Reset}" -ForegroundColor Red
}

Write-Host ""
Write-Host "${Green}=== All Tests Complete ===${Reset}" -ForegroundColor Green
