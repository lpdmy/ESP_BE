# Script to fix encoding issues in ActivitytService.cs
$filePath = "Application\Services\ActivityService\ActivitytService.cs"
$content = Get-Content $filePath -Raw -Encoding UTF8

# Replace incorrect Vietnamese strings with correct ones
$content = $content -replace '"ang di\?n ra"', '"Đang diễn ra"'
$content = $content -replace '"\? k\?t th\?c"', '"Đã kết thúc"'
$content = $content -replace '"ang c\?p nh\?t"', '"Đang cập nhật"'

# Save the file
$content | Set-Content $filePath -Encoding UTF8 -NoNewline

Write-Host "Fixed encoding issues in ActivitytService.cs"

