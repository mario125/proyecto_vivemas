var selectdFile;
document.getElementById('inputArchivo').addEventListener('change', (event) => {
    selectdFile = event.target.files[0];
});
document.getElementById('subirExcel').addEventListener('click', () => {
    if (selectdFile) {        
        var fileReader = new FileReader();
        fileReader.onload = function (event) {
            var data = event.target.result;
            var workbook = XLSX.read(data, {
                type: "binary"
            });
            workbook.SheetNames.forEach(sheet => {
                let rowObject = XLSX.utils.sheet_to_row_object_array(
                    workbook.Sheets[sheet]
                );
                let jsonObject = JSON.stringify(rowObject);                
                document.getElementById("jsonData").innerHTML = jsonObject;               
                var data = new Object();
                data.filas = rowObject;
                realizarMigracion(data);                
            });
        };
        fileReader.readAsBinaryString(selectdFile);
    }
});

const realizarMigracion = async (data) => {
    
    /*const response = await axios.post('MigrarPagosAction', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });*/

    fetch("MigrarContratosAction", {
        method: 'POST',
        body: JSON.stringify(data),
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(res => res.text())
        .then(response => {
            console.log(response);
        })
        .catch(error => console.log(error))
}