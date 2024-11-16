module.exports = function(RED) {
    function SpendLessConfigNode(config) {
        RED.nodes.createNode(this, config);

        this.baseUrl = config.baseUrl;
        this.port = config.port;
    }

    RED.nodes.registerType("spend-less-config", SpendLessConfigNode);
};

