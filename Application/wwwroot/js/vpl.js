
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
        let el = document.getElementById(elementId);
        if(el == null) return;
        el.innerHTML = marked.parse(rawBody);
        Prism.highlightAll();
    },

    
    showModal: function (elementId) {
        var modal =  bootstrap.Modal.getInstance('#'+elementId);
        if(modal == null) modal = new bootstrap.Modal(document.getElementById(elementId), {
            focus: true,
        })
        modal.show();
    },
    
    hideModal: function (elementId) {
        var modal = bootstrap.Modal.getInstance('#'+elementId);
        modal.hide();
    },

    enableTooltips: function () {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))  
    }
}
