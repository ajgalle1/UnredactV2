# UnredactV2

Introducing the FOIA Hero, a cutting-edge web application designed to support FOIA officers in ensuring the accuracy and completeness of their responses.

Why it matters
The Freedom of Information Act (FOIA) is a foundational document in American democracy, laying out the government's commitment to transparency. It is often quoted that "democracy dies in the dark," and FOIA is the leading mechanism that ensures confidence in open, accountable governance.
However, not all information is open for public inspection, such as trade secrets, certain Personally Identifiable Information, or military capabilities and plans. A FOIA analyst must be vigilant in keeping secrets secret.

Complicating matters, reports of bot driven FOIA requests threaten to overburden the already strained agencies.

FOIA Hero helps civil servants in fulfilling their obligations to promote transparency while safeguarding sensitive material.  

Key Features:
Automated Error Detection: Leverages Azure AI Language services and Vision Services to meticulously process and analyze FOIA officer work, flagging potentially ineffective redactions, inadvertent PII leaks, and more.

Real-Time Feedback: Receive instant feedback and suggestions for corrections, allowing FOIA officers to make necessary adjustments quickly and efficiently.

Comprehensive Reporting: Generate detailed reports that highlight areas of concern and track improvements over time, fostering a culture of continuous improvement.

Technologies Leveraged
Blazor
C#
Visual Studio
Azure AI Language Services
Azure AI Vision Services
Azure AI Sentiment Analysis
Azure AI OCR
Azure AI PII Detection
Azure Blob Storage
Azure WebApps

This was developed for a timelimited event, and I ran out off time to implement everything I wanted to.  Still, it is useful as a thumbnail proof of concept.  This was actually my first time using many of these technologies and linking them together, so it was a great experience in how it all works together. 

Bug List:
  1) The upload link in the app to blob storage is inoperable. Not enough time to troubleshoot.
  2) The function app's function does trigger upon manual upload in the portal or using Azure Storage Explorer
  3) The metadata updating to the blobs in the container is flawed and needs attention.
  4) The "redaction ineffective" marking has not yet been implemented.
  5) To demonstrate the intent of the project, logic was added to randomly assign flags to files in the blob to illustrate what it would look like to the reviewer.
  6) Security and user experience can be improved. For example, validation can be added to the upload function to detect formats that are not accepted by Document Intelligence.
  7) User accounts and authentication could be added, but this would make it difficult to demo, so was ommited.
     
