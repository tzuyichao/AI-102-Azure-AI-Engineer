# Quickstart: Language Understanding (LUIS) client libraries and REST API

        LUIS will be retired on October 1st 2025 and starting April 1st 2023 you will not be able to create new LUIS resources. We recommend migrating your LUIS applications to conversational language understanding to benefit from continued product support and multilingual capabilities.

## Authoring Object model

The Language Understanding (LUIS) authoring client is a [LUISAuthoringClient](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.cognitiveservices.language.luis.authoring.luisauthoringclient) object that authenticates to Azure, which contains your authoring key.

### Code Examples for authoring

* Apps - create, delete, publish
* Example utterances - add, delete by ID
* Features - manage phrase lists
* Model - manage intents and entities
* Pattern - manage patterns
* Train - train the app and poll for training status
* Versions - manage with clone, export, and delete

## Prediction Object model

The Language Understanding (LUIS) prediction runtime client is a [LUISRuntimeClient](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.cognitiveservices.language.luis.runtime.luisruntimeclient) object that authenticates to Azure, which contains your resource key.

## Source

* [MS Learn](https://learn.microsoft.com/en-us/azure/cognitive-services/luis/client-libraries-rest-api?tabs=linux&pivots=programming-language-csharp)