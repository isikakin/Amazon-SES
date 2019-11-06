var AWS = require('aws-sdk');

const s3 = new AWS.S3({});

module.exports.handler = (event, context, callback) => {

    var bucketKey = event.Records[0].body;

    var params = {
        Bucket: 'akin-email-test',
        Key: bucketKey + '.json'
    }

    console.log(params);
    try {
        s3.getObject(params, function (err, data) {
            if (err) {
                console.log("errorr", err);
                returnResponse(false, "Error", JSON.stringify(err));
            }
            else {
                console.log('getobject from s3');
                console.log("data", data.Body.toString('utf-8'));
                let mailData = JSON.parse(data.Body.toString('utf-8'));
                sendEmail(mailData);
            }
        });
    }
    catch (err) {
        console.log('err', JSON.stringify(err.message));
        returnResponse(false, "Error", JSON.stringify(err.message));
    }

    function sendEmail(mailData) {
        console.log("sendEmail");
        const fromEmail = mailData.FromEmail;

        const fullName = mailData.FullName;

        const htmlBody = `
    <!DOCTYPE html>
    <html>
      <head>
      </head>
      <body>
      <p>Hi ${fullName}, </p>
      <br>
      <p>This email sent from amazon ses.</p>
      <br>
      <br>
      <br>
      </body>
    </html>
  `;

        // Create sendEmail params
        const params = {
            Destination: {
                ToAddresses: [mailData.ToEmail]
            },
            Message: {
                Body: {
                    Html: {
                        Charset: "UTF-8",
                        Data: htmlBody
                    }
                },
                Subject: {
                    Charset: "UTF-8",
                    Data: mailData.Title
                }
            },
            Source: `Akın Işık <${fromEmail}>`
        };

        // Create the promise and SES service object
        const sendPromise = new AWS.SES({ apiVersion: "2010-12-01" })
            .sendEmail(params)
            .promise();

        // Handle promise's fulfilled/rejected states
        sendPromise
            .then(data => {
                console.log(data.MessageId);
                returnResponse(true, "Success", null);
            })
            .catch(err => {
                console.error(err, err.stack);
                returnResponse(false, "Error while during send email", null);
            });
    }

    function returnResponse(status, statusMessage, data) {
        var response = {};
        response.status = status;
        response.statusMessage = statusMessage;
        response.data = data;

        return response;
    }
};