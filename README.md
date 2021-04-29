# Validating a Text Request Webhook

This example code shows how to validate that a webhook callback actually originated from Text Request. 

Each time that Text Request sends a webhook callback, it will calculate and include x-tr-webhook-signature and x-tr-webhook-timestamp request headers. These headers are created 
by:

* Appending the message timestamp in ticks and the callback body, separated by a period. Example: if the ticks are 1619448468 and the JSON request body was { "message": "hello" }, then the fully appended string would be 1619448468.{ "message": "hello" }
* The input is hashed with your Text Request account's API key using HMAC SHA 256. 

When you receive a webhook callback from Text Request, you can perform the same calculations. If your hash matches the x-tr-webhook-signature, then you know that the original callback timestamp and body originated from Text Request and have not been altered. You can then compare the timestamps in ticks to your server's current time in UTC in ticks to confirm that the message is not an older message that is being replayed. You will need to choose your own valid sliding window to account for processing time and clock skew. 

This example code demonstrates how to perform these calculations in C#. To do so, you will need to:

1. Capture a Text Request webhook callback's body and save it in a file. Let's call the file input.txt. 
2. Find your Text Request account's API key. This can be found by logging into the Text Request web app (https://app.textrequest.com/app/queue) as an administrator, and going to Integrations -> API Keys & Webhooks. 
3. Get the x-tr-webhook-signature and x-tr-webhook-timestamp request header values. 
4. Run the example program with the arguments of <fully qualified name and path to input.txt> <your Text Request API key> <x-tr-webhook-signature value> <x-tr-webhook-timestamp value>. 

Note that this example program uses a sliding time window of five minutes from when the webhook was sent. If you run this program outside of that time window, the timestamp validation will fail. 
