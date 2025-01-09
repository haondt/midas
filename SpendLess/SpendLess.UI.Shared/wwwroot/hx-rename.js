(function () {
    htmx.defineExtension('rename', {
        onEvent: function (name, evt) {
            if (name !== "htmx:configRequest")
                return;

            const parent = evt.detail.elt;
            const renameAttr = parent.getAttribute('hx-rename');
            if (!renameAttr)
                return;

            const renamePairs = renameAttr.split(',');
            renamePairs.forEach(pair => {
                const [k1, k2] = pair.split(':');
                if (!(evt.detail.parameters.has(k1)))
                    return;
                evt.detail.parameters[k2] = evt.detail.parameters[k1];
                delete evt.detail.parameters[k1];
            });
        }
    })
})();
