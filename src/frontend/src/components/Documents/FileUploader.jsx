import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import { Dimmer, Header, Icon, Loader, Segment } from 'semantic-ui-react';
import { uploadFileRequest, uploadProgressSelector } from '../../ducks/documents';
import { toast } from 'react-toastify';

const FileUploader = ({ document, onChange }) => {
    const [active, setActive] = useState(false);
    const { t } = useTranslation();
    const dispatch = useDispatch();
    const loading = useSelector(state => uploadProgressSelector(state));

    const onDragEnter = () => {
        setActive(true);
    };

    const onDragLeave = () => {
        setActive(false);
    };

    const onDragOver = e => {
        e.preventDefault();
    };

    const onDrop = e => {
        e.preventDefault();
        setActive(false);
        uploadFile(e, e.dataTransfer.files[0]);
    };

    const uploadFile = (e, file) => {
        // this.props.onChange();
        let form = new FormData();
        file = file || e.target.files[0];

        if (file && file.size <= 10000000) {
            form.append('formFile', file);

            dispatch(
                uploadFileRequest({
                    form,
                    fileName: file.name,
                    callbackSuccess: id => {
                        onChange(id, file.name);
                    },
                }),
            );
        } else if (file.size > 10000000) {
            toast.error(t('error_file_size'));
        }
    };

    const loaded = document && document.fileId,
        name = document ? document.name : '',
        extension = loaded ? name.substr(name.lastIndexOf('.') + 1) : '',
        isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(extension.toLowerCase()),
        src = loaded ? `/api/files/${loaded}` : '',
        labelClass = `uploader ${loaded && 'loaded'} ${active && 'dragndrop'}`;

    let preview = loading ? (
        <Dimmer active inverted>
            <Loader size="large">{t('Loading')}</Loader>
        </Dimmer>
    ) : (
        <Segment placeholder>
            <Header icon>
                <Icon name={loaded ? 'file outline' : 'upload'} />
            </Header>
            <p>{name}</p>
        </Segment>
    );

    if (loaded && isImage) preview = <img src={src} className={loaded && 'loaded'} />;

    return (
        <label
            className={labelClass}
            onDragEnter={onDragEnter}
            onDragLeave={onDragLeave}
            onDragOver={onDragOver}
            onDrop={onDrop}
        >
            {preview}

            <input type="file" onChange={uploadFile} />
        </label>
    );
};

export default FileUploader;
