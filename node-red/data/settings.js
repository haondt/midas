module.exports = {
    flowFile: 'flows.json',
    flowFilePretty: true,
    uiPort: process.env.PORT || 1880,
    /** The maximum size of HTTP request that will be accepted by the runtime api.
     * Default: 5mb
     */
    //apiMaxLength: '5mb',

    /** The following property can be used to pass custom options to the Express.js
     * server used by Node-RED. For a full list of available options, refer
     * to http://expressjs.com/en/api.html#app.settings.table
     */
    //httpServerOptions: { },

    /** Configure diagnostics options
     * - enabled:  When `enabled` is `true` (or unset), diagnostics data will
     *   be available at http://localhost:1880/diagnostics
     * - ui: When `ui` is `true` (or unset), the action `show-system-info` will
     *   be available to logged in users of node-red editor
    */
    diagnostics: {
        /** enable or disable diagnostics endpoint. Must be set to `false` to disable */
        enabled: true,
        /** enable or disable diagnostics display in the node-red editor. Must be set to `false` to disable */
        ui: true,
    },
    /** Configure runtimeState options
     * - enabled:  When `enabled` is `true` flows runtime can be Started/Stoped
     *   by POSTing to available at http://localhost:1880/flows/state
     * - ui: When `ui` is `true`, the action `core:start-flows` and
     *   `core:stop-flows` will be available to logged in users of node-red editor
     *   Also, the deploy menu (when set to default) will show a stop or start button
     */
    runtimeState: {
        /** enable or disable flows/state endpoint. Must be set to `false` to disable */
        enabled: false,
        /** show or hide runtime stop/start options in the node-red editor. Must be set to `false` to hide */
        ui: false,
    },
    /** Configure the logging output */
    logging: {
        /** Only console logging is currently supported */
        console: {
            /** Level of logging to be recorded. Options are:
             * fatal - only those errors which make the application unusable should be recorded
             * error - record errors which are deemed fatal for a particular request + fatal errors
             * warn - record problems which are non fatal + errors + fatal errors
             * info - record information about the general running of the application + warn + error + fatal errors
             * debug - record information which is more verbose than info + info + warn + error + fatal errors
             * trace - record very detailed logging + debug + info + warn + error + fatal errors
             * off - turn off all logging (doesn't affect metrics or audit)
             */
            level: "info",
            /** Whether or not to include metric events in the log output */
            metrics: false,
            /** Whether or not to include audit events in the log output */
            audit: false
        }
    },

    exportGlobalContextKeys: false,

    /** Configure how the runtime will handle external npm modules.
     * This covers:
     *  - whether the editor will allow new node modules to be installed
     *  - whether nodes, such as the Function node are allowed to have their
     * own dynamically configured dependencies.
     * The allow/denyList options can be used to limit what modules the runtime
     * will install/load. It can use '*' as a wildcard that matches anything.
     */
    externalModules: {
        autoInstall: true,   /** Whether the runtime will attempt to automatically install missing modules */
        // autoInstallRetry: 30, /** Interval, in seconds, between reinstall attempts */
        palette: {              /** Configuration for the Palette Manager */
            allowInstall: true, /** Enable the Palette Manager in the editor */
            allowUpdate: true,  /** Allow modules to be updated in the Palette Manager */
            allowUpload: true,  /** Allow module tgz files to be uploaded and installed */
            allowList: ['*'],
            denyList: [],
            allowUpdateList: ['*'],
            denyUpdateList: []
        },
        // modules: {              /** Configuration for node-specified modules */
        //     allowInstall: true,
        //     allowList: [],
        //     denyList: []
        // }
    },


/*******************************************************************************
 * Editor Settings
 *  - disableEditor
 *  - editorTheme
 ******************************************************************************/

    /** The following property can be used to disable the editor. The admin API
     * is not affected by this option. To disable both the editor and the admin
     * API, use either the httpRoot or httpAdminRoot properties
     */
    //disableEditor: false,

    /** Customising the editor
     * See https://nodered.org/docs/user-guide/runtime/configuration#editor-themes
     * for all available options.
     */
    editorTheme: {
        /** The following property can be used to set a custom theme for the editor.
         * See https://github.com/node-red-contrib-themes/theme-collection for
         * a collection of themes to chose from.
         */
        theme: "tokyo-night",

        /** To disable the 'Welcome to Node-RED' tour that is displayed the first
         * time you access the editor for each release of Node-RED, set this to false
         */
        tours: true,

        palette: {
            /** The following property can be used to order the categories in the editor
             * palette. If a node's category is not in the list, the category will get
             * added to the end of the palette.
             * If not set, the following default order is used:
             */
            categories: ['spendless'],
        },

        projects: {
            /** To enable the Projects feature, set this value to true */
            enabled: false,
            workflow: {
                /** Set the default projects workflow mode.
                 *  - manual - you must manually commit changes
                 *  - auto - changes are automatically committed
                 * This can be overridden per-user from the 'Git config'
                 * section of 'User Settings' within the editor
                 */
                mode: "manual"
            }
        },

        codeEditor: {
            /** Select the text editor component used by the editor.
             * As of Node-RED V3, this defaults to "monaco", but can be set to "ace" if desired
             */
            lib: "monaco",
            options: {
                /** The follow options only apply if the editor is set to "monaco"
                 *
                 * theme - must match the file name of a theme in
                 * packages/node_modules/@node-red/editor-client/src/vendor/monaco/dist/theme
                 * e.g. "tomorrow-night", "upstream-sunburst", "github", "my-theme"
                 */
                theme: "tokyo-night",
                /** other overrides can be set e.g. fontSize, fontFamily, fontLigatures etc.
                 * for the full list, see https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.IStandaloneEditorConstructionOptions.html
                 */
                //fontSize: 14,
                //fontFamily: "Cascadia Code, Fira Code, Consolas, 'Courier New', monospace",
                //fontLigatures: true,
            }
        }
    },

/*******************************************************************************
 * Node Settings
 *  - fileWorkingDirectory
 *  - functionGlobalContext
 *  - functionExternalModules
 *  - nodeMessageBufferMaxLength
 *  - ui (for use with Node-RED Dashboard)
 *  - debugUseColors
 *  - debugMaxLength
 *  - execMaxBufferSize
 *  - httpRequestTimeout
 *  - mqttReconnectTime
 *  - serialReconnectTime
 *  - socketReconnectTime
 *  - socketTimeout
 *  - tcpMsgQueueSize
 *  - inboundWebSocketTimeout
 *  - tlsConfigDisableLocalFiles
 *  - webSocketNodeVerifyClient
 ******************************************************************************/

    /** The working directory to handle relative file paths from within the File nodes
     * defaults to the working directory of the Node-RED process.
     */
    //fileWorkingDirectory: "",

    /** Allow the Function node to load additional npm modules directly */
    functionExternalModules: true,

    /** The following property can be used to set predefined values in Global Context.
     * This allows extra node modules to be made available with in Function node.
     * For example, the following:
     *    functionGlobalContext: { os:require('os') }
     * will allow the `os` module to be accessed in a Function node using:
     *    global.get("os")
     */
    functionGlobalContext: {
        // os:require('os'),
    },

    /** The maximum number of messages nodes will buffer internally as part of their
     * operation. This applies across a range of nodes that operate on message sequences.
     * defaults to no limit. A value of 0 also means no limit is applied.
     */
    //nodeMessageBufferMaxLength: 0,

    /** If you installed the optional node-red-dashboard you can set it's path
     * relative to httpNodeRoot
     * Other optional properties include
     *  readOnly:{boolean},
     *  middleware:{function or array}, (req,res,next) - http middleware
     *  ioMiddleware:{function or array}, (socket,next) - socket.io middleware
     */
    //ui: { path: "ui" },

    /** Colourise the console output of the debug node */
    //debugUseColors: true,

    /** The maximum length, in characters, of any message sent to the debug sidebar tab */
    debugMaxLength: 1000,

    /** Maximum buffer size for the exec node. Defaults to 10Mb */
    //execMaxBufferSize: 10000000,

    /** Timeout in milliseconds for HTTP request connections. Defaults to 120s */
    //httpRequestTimeout: 120000,

    /** Retry time in milliseconds for MQTT connections */
    mqttReconnectTime: 15000,

    /** Retry time in milliseconds for Serial port connections */
    serialReconnectTime: 15000,

    /** Retry time in milliseconds for TCP socket connections */
    //socketReconnectTime: 10000,

    /** Timeout in milliseconds for TCP server socket connections. Defaults to no timeout */
    //socketTimeout: 120000,

    /** Maximum number of messages to wait in queue while attempting to connect to TCP socket
     * defaults to 1000
     */
    //tcpMsgQueueSize: 2000,

    /** Timeout in milliseconds for inbound WebSocket connections that do not
     * match any configured node. Defaults to 5000
     */
    //inboundWebSocketTimeout: 5000,

    /** To disable the option for using local files for storing keys and
     * certificates in the TLS configuration node, set this to true.
     */
    //tlsConfigDisableLocalFiles: true,

    /** The following property can be used to verify websocket connection attempts.
     * This allows, for example, the HTTP request headers to be checked to ensure
     * they include valid authentication information.
     */
    //webSocketNodeVerifyClient: function(info) {
    //    /** 'info' has three properties:
    //    *   - origin : the value in the Origin header
    //    *   - req : the HTTP request
    //    *   - secure : true if req.connection.authorized or req.connection.encrypted is set
    //    *
    //    * The function should return true if the connection should be accepted, false otherwise.
    //    *
    //    * Alternatively, if this function is defined to accept a second argument, callback,
    //    * it can be used to verify the client asynchronously.
    //    * The callback takes three arguments:
    //    *   - result : boolean, whether to accept the connection or not
    //    *   - code : if result is false, the HTTP error status to return
    //    *   - reason: if result is false, the HTTP reason string to return
    //    */
    //},
}
