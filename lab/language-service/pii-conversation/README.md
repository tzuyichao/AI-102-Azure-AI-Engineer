# PII: Hoe to call for conversations

## Submit transcripts using speech-to-text

```
curl -i -X POST https://your-language-endpoint-here/language/analyze-conversations/jobs?api-version=2022-05-15-preview \
-H "Content-Type: application/json" \
-H "Ocp-Apim-Subscription-Key: your-key-here" \
-d \
' 
{
    "displayName": "Analyze conversations from xxx",
    "analysisInput": {
        "conversations": [
            {
                "id": "23611680-c4eb-4705-adef-4aa1c17507b5",
                "language": "en",
                "modality": "transcript",
                "conversationItems": [
                    {
                        "participantId": "agent_1",
                        "id": "8074caf7-97e8-4492-ace3-d284821adacd",
                        "text": "Good morning.",
                        "lexical": "good morning",
                        "itn": "good morning",
                        "maskedItn": "good morning",
                        "audioTimings": [
                            {
                                "word": "good",
                                "offset": 11700000,
                                "duration": 2100000
                            },
                            {
                                "word": "morning",
                                "offset": 13900000,
                                "duration": 3100000
                            }
                        ]
                    },
                    {
                        "participantId": "agent_1",
                        "id": "0d67d52b-693f-4e34-9881-754a14eec887",
                        "text": "Can I have your name?",
                        "lexical": "can i have your name",
                        "itn": "can i have your name",
                        "maskedItn": "can i have your name",
                        "audioTimings": [
                            {
                                "word": "can",
                                "offset": 44200000,
                                "duration": 2200000
                            },
                            {
                                "word": "i",
                                "offset": 46500000,
                                "duration": 800000
                            },
                            {
                                "word": "have",
                                "offset": 47400000,
                                "duration": 1500000
                            },
                            {
                                "word": "your",
                                "offset": 49000000,
                                "duration": 1500000
                            },
                            {
                                "word": "name",
                                "offset": 50600000,
                                "duration": 2100000
                            }
                        ]
                    },
                    {
                        "participantId": "customer_1",
                        "id": "08684a7a-5433-4658-a3f1-c6114fcfed51",
                        "text": "Sure that is John Doe.",
                        "lexical": "sure that is john doe",
                        "itn": "sure that is john doe",
                        "maskedItn": "sure that is john doe",
                        "audioTimings": [
                            {
                                "word": "sure",
                                "offset": 5400000,
                                "duration": 6300000
                            },
                            {
                                "word": "that",
                                "offset": 13600000,
                                "duration": 2300000
                            },
                            {
                                "word": "is",
                                "offset": 16000000,
                                "duration": 1300000
                            },
                            {
                                "word": "john",
                                "offset": 17400000,
                                "duration": 2500000
                            },
                            {
                                "word": "doe",
                                "offset": 20000000,
                                "duration": 2700000
                            }
                        ]
                    }
                ]
            }
        ]
    },
    "tasks": [
        {
            "taskName": "analyze 1",
            "kind": "ConversationalPIITask",
            "parameters": {
                "modelVersion": "2022-05-15-preview",
                "redactionSource": "text",
                "includeAudioRedaction": true,
                "piiCategories": [
                    "all"
                ]
            }
        }
    ]
}
`

```

## Submit text chats

```
curl -i -X POST https://your-language-endpoint-here/language/analyze-conversations/jobs?api-version=2022-05-15-preview \
-H "Content-Type: application/json" \
-H "Ocp-Apim-Subscription-Key: your-key-here" \
-d \
' 
{
    "displayName": "Analyze conversations from xxx",
    "analysisInput": {
        "conversations": [
            {
                "id": "23611680-c4eb-4705-adef-4aa1c17507b5",
                "language": "en",
                "modality": "text",
                "conversationItems": [
                    {
                        "participantId": "agent_1",
                        "id": "8074caf7-97e8-4492-ace3-d284821adacd",
                        "text": "Good morning."
                    },
                    {
                        "participantId": "agent_1",
                        "id": "0d67d52b-693f-4e34-9881-754a14eec887",
                        "text": "Can I have your name?"
                    },
                    {
                        "participantId": "customer_1",
                        "id": "08684a7a-5433-4658-a3f1-c6114fcfed51",
                        "text": "Sure that is John Doe."
                    }
                ]
            }
        ]
    },
    "tasks": [
        {
            "taskName": "analyze 1",
            "kind": "ConversationalPIITask",
            "parameters": {
                "modelVersion": "2022-05-15-preview"
            }
        }
    ]
}
`
```

Submit Job result的link會在submit job的response header的</code>operation-location</code>裡面。

## Get the result

```
curl -X GET    https://your-language-endpoint/language/analyze-conversations/jobs/my-job-id \
-H "Content-Type: application/json" \
-H "Ocp-Apim-Subscription-Key: your-key-here"
```

Rsult可能像下列這樣

```
{
    "jobId": "184d4724-7aec-47f5-8b54-d5f2720323ae",
    "lastUpdatedDateTime": "2023-03-23T05:40:50Z",
    "createdDateTime": "2023-03-23T05:40:48Z",
    "expirationDateTime": "2023-03-24T05:40:48Z",
    "status": "succeeded",
    "errors": [],
    "displayName": "Analyze conversations from xxx",
    "tasks": {
        "completed": 1,
        "failed": 0,
        "inProgress": 0,
        "total": 1,
        "items": [
            {
                "kind": "conversationalPIIResults",
                "taskName": "analyze 1",
                "lastUpdateDateTime": "2023-03-23T05:40:50.2710704Z",
                "status": "succeeded",
                "results": {
                    "conversations": [
                        {
                            "id": "23611680-c4eb-4705-adef-4aa1c17507b5",
                            "conversationItems": [
                                {
                                    "id": "8074caf7-97e8-4492-ace3-d284821adacd",
                                    "redactedContent": {
                                        "text": "Good morning."
                                    },
                                    "entities": []
                                },
                                {
                                    "id": "0d67d52b-693f-4e34-9881-754a14eec887",
                                    "redactedContent": {
                                        "text": "Can I have your name?"
                                    },
                                    "entities": []
                                },
                                {
                                    "id": "08684a7a-5433-4658-a3f1-c6114fcfed51",
                                    "redactedContent": {
                                        "text": "Sure that is ********."
                                    },
                                    "entities": [
                                        {
                                            "text": "John Doe",
                                            "category": "Name",
                                            "offset": 13,
                                            "length": 8,
                                            "confidenceScore": 0.94
                                        }
                                    ]
                                }
                            ],
                            "warnings": []
                        }
                    ],
                    "errors": [],
                    "modelVersion": "2022-05-15-preview"
                }
            }
        ]
    }
}
```


[MS Learn: How to guides](https://learn.microsoft.com/en-us/azure/cognitive-services/language-service/personally-identifiable-inaformation/how-to-call-for-conversations?tabs=rest-api)