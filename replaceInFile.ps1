$sourceFile=$args[0]
$word = $args[1]
$replacement = $args[2]
$tardetFile=$args[0]
if($args[3]) {$tardetFile=$args[3]}

$newContent = (Get-Content $sourceFile) | Foreach-Object {$_ -replace $word ,$replacement }

Set-Content -Path $tardetFile -Value $newContent


