import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import { Icon, Image, Modal } from 'semantic-ui-react';
import { documentTypesSelector, getDocumentTypesRequest } from '../../ducks/documents';

const DocView = ({ onClick, children, document }) => {
    let [modalOpen, setModalOpen] = useState(false);
    const { t } = useTranslation();
    const dispatch = useDispatch();

    useEffect(() => {
        dispatch(getDocumentTypesRequest());
    }, []);

    const documentTypes = useSelector(state => documentTypesSelector(state));

    const type = document && documentTypes.find(type => type.value === document.typeId);
    const typeText = type && type.text;

    const handleOpen = () => {
        onClick && onClick();
        setModalOpen(true);
    };

    const handleClose = () => setModalOpen(false);

    if (document && document.fileId) {
        const src = `/api/files/${document.fileId}`,
            name = document.name || '',
            extension = name.substr(name.lastIndexOf('.') + 1),
            isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(extension.toLowerCase());

        let image = isImage ? (
            <div
                className="image-container"
                style={{ background: `url(${src}) no-repeat center center` }}
                onClick={handleOpen}
            />
        ) : (
            <div className="image-container">
                <a target="_blanc" href={src}>
                    <Icon name="file outline" />
                </a>
            </div>
        );
        let inner = (
            <div>
                {image}

                <div className="file-info">
                    <u>
                        {typeText && typeText.length > 19
                            ? `${typeText.slice(0, 15)}...`
                            : typeText}
                    </u>
                    <div>
                        {document.name && document.name.length > 15
                            ? `${document.name.slice(0, 12)}..${document.name.slice(-5, -1)}`
                            : document.name}
                    </div>
                </div>
            </div>
        );

        return (
            <div className="file-item">
                <Modal
                    className="top-layer"
                    trigger={inner}
                    open={modalOpen}
                    onClose={handleClose}
                    closeOnEscape
                    closeOnDimmerClick={false}
                    basic
                >
                    <Modal.Content>
                        <Image fluid className="image-fit" src={src} onClick={handleClose} />
                    </Modal.Content>
                </Modal>
                {children}
            </div>
        );
    }
    return <div className="file-item file-item-add">{children}</div>;
};

export default DocView;
