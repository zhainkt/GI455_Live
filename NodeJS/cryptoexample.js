const crypto = require('crypto');

const hashingSecret = "ARandomSecretKey";
const plainText = "Hello World!";

const hashedStr = crypto.createHmac('sha256', hashingSecret)
                        .update(plainText)
                        .digest('hex');

console.log(hashedStr);