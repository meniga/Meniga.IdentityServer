Remove-Item $env:USERPROFILE\.nuget\packages\Meniga.identityserver\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\Meniga.identityserver.storage\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\Meniga.identityserver.entityframework\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\Meniga.identityserver.entityframework.storage\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\Meniga.identityserver.aspnetidentity\ -Recurse -ErrorAction SilentlyContinue 

Remove-Item $env:USERPROFILE\.nuget\packages\Meniga.identitymodel\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\IdentityModel.AspNetCore.OAuth2Introspection\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\IdentityModel.AspNetCore.AccessTokenValidation\ -Recurse -ErrorAction SilentlyContinue 