# CLI for key vault and service principals

```
az keyvault create --name "kv20230315" --resource-group "cognitive-services-resource-group" --location southeastasia

az keyvault secret set --vault-name "kv20230315" --name "Cognitive-Services-Key" --value "<Cognitive_service_subscription_key>"

az ad sp create-for-rbac -n "api://appName" --role owner --scopes subscriptions/<subscriptionID>/resourceGroups/<ResourceGroupName>

az ad sp show --id <appId> --query id --out tsv

az keyvault set-policy --resource-group "cognitive-services-resource-group" --name "kv20230315" --object-id <objectId> --secret-permissions get list 
```

踹完記得刪除資源