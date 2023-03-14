$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("Ocp-Apim-Subscription-Key", "<YOUR KEY>")
$headers.Add("Content-Type", "application/json")

$body = "{
`n    `"documents`": [
`n        {`"id`": 1,
`n        `"text`": `"hello`"
`n        }
`n    ]
`n}"

$response = Invoke-RestMethod 'https://<YOUR ENDPOINT>.api.cognitive.microsoft.com/text/analytics/v3.0/languages' -Method 'POST' -Headers $headers -Body $body
$response | ConvertTo-Json