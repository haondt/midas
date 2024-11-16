module.exports = function(RED) {
    function SetSourceAccountNode(config) {
        RED.nodes.createNode(this,config);

        var node = this;

        node.on('input', function(msg) {
            msg.payload = msg.payload.toLowerCase();
            node.send(msg);
        });

    }

    const serviceUrl = process.env['SPEND_LESS_API_URL'];
    RED.nodes.registerType("set-source-account", SetSourceAccountNode, {
        settings: {
            setSourceAccountServiceUrl: { value: serviceUrl, exportable: true }
        }
    });
}
