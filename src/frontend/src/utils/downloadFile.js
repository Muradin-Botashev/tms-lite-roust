export default result => {
    const { data, headers } = result;
    let headerLine = headers['content-disposition'];
    let startFileNameIndex = headerLine.indexOf('filename=') + 10;
    let endFileNameIndex = headerLine.lastIndexOf(';') - 1;
    let filename = headerLine.substring(startFileNameIndex, endFileNameIndex);

    const link = document.createElement('a');
    link.href = URL.createObjectURL(new Blob([data], { type: data.type }));
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
}
