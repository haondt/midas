module.exports = function(RED) {
    function LowerCaseNode(config) {
        RED.nodes.createNode(this,config);

        var node = this;

        // const axios = require('axios');
        // const url = config.url;
        // this.serviceUrl = config.serviceUrl;
        // conse serviceUrl = process.env[node.serviceUrl] || node.serviceUrl;

        node.on('input', function(msg) {
            msg.payload = msg.payload.toLowerCase();
            node.send(msg);
        });

    }

    const serviceUrl = process.env['SPEND_LESS_API_URL'];
    RED.nodes.registerType("lower-case",LowerCaseNode, {
        settings: {
            lowerCaseServiceUrl: { value: serviceUrl, exportable: true }
        }
    });
}
