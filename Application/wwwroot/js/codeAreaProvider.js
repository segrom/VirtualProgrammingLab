
window.codeFunctions = {
    addCodeAreaProvider: function (editorId) {
        codeInput.registerTemplate("syntax-highlighted", codeInput.templates.prism(Prism, [
            new codeInput.plugins.Indent(true, 4), // 2 spaces indentation
            new codeInput.plugins.AutoCloseBrackets(),
            // new codeInput.plugins.Autocomplete()
        ]));
        
        let codeInputId = `#CodeInput${editorId}`
        let codeProviderId = `#Provider${editorId}`
        
        let codeArea = document.querySelector(`${codeInputId} > textarea`)
        let codeProvider = document.querySelector(codeProviderId);

        codeArea.addEventListener('change', function () {
            codeProvider.value = codeArea.value;
            codeProvider.dispatchEvent(new Event('change'));
        })
        codeArea.addEventListener('keyup', function () {
            codeProvider.value = codeArea.value;
            codeProvider.dispatchEvent(new Event('change'));
        })
    },
    
    parseMarkArticle: function (rawBody, elementId) {
            document.getElementById(elementId).innerHTML =
            marked.parse(rawBody);
    }
}
