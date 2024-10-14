# SMtoGC

Used to convert a calendar in [Schedule maker](https://schedulemaker.io) to a calendar in google calendar.<br />

How to use: <br />
  Make the calendar in [Schedule maker](schedulemaker.io) <br />
  click Save then Export file <br />
  Copy paste the contents of the JSON file to into the json string in the code while you're there, change the TimeZone variable in the CreateNewGCEvent Method! <br /> <br />
  
  Now the complicated part, you need to go to [Google cloud](console.cloud.google.com) then create a project  <br />
  you need to add the google calendar API, create a OAUTH 2.0 credential and make sure you add yourself as a tester, and copy paste the client and secret ID into said variable in the code <br />
  Here is a [tutorial](https://youtu.be/w6rzVKBsB3A?si=98TEaJRgnYoUKR7d) <br /> <br />

  *optional:* <br />
  Create a calendar, and copy paste the calendar ID to said variable in the code  <br />

  Now all is needed is to press run.<br />

  

