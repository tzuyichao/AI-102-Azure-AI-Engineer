# CLI 

```
az login

# create resource group
az group create --name cognitive-services-resource-group --location southeastasia

# Create multi-services cognitive service account
az cognitiveservices account create --name acs20230315 --resource-group cognitive-services-resource-group  --kind CognitiveServices --sku S0 --location southeastasia --yes

# Get Key
az cognitiveservices account keys list  --name acs20230315 --resource-group cognitive-services-resource-group

# Get Endpoint
az cognitiveservices account show --name acs20230315 --resource-group cognitive-services-resource-group --query properties.endpoint

```