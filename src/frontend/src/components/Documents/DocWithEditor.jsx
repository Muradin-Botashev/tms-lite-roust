import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import { Button, Confirm, Form, Icon, Modal, Popup } from 'semantic-ui-react';
import FileUploader from './FileUploader';
import DocView from './DocView';
import WebCamUploader from './WebCamUploader';
import './style.scss';
import {
    addDocumentRequest,
    deleteDocumentRequest,
    documentTypesSelector,
    downloadDocumentRequest,
    editDocumentRequest,
    getDocumentTypesRequest,
} from '../../ducks/documents';
import { editCardRequest } from '../../ducks/gridCard';

const DocWithEditor = ({
    okButtonText,
    titleText,
    document: currentDocument,
    gridName,
    cardId,
    getDocuments,
    isEditPermissions,
                           load,
}) => {
    let [modalOpen, setModalOpen] = useState(false);
    let [confirmation, setConfirmation] = useState({ open: false });
    let [document, setDocument] = useState(
        currentDocument ? Object.assign({}, currentDocument) : null,
    );

    const { t } = useTranslation();
    const dispatch = useDispatch();

    const documentTypes = useSelector(state => documentTypesSelector(state));

    useEffect(
        () => {
            setDocument(currentDocument ? Object.assign({}, currentDocument) : null);
        },
        [currentDocument],
    );

    const handleOpen = () => {
        dispatch(getDocumentTypesRequest());
        setModalOpen(true);
    };

    const handleClose = () => {
        load && load();
        setDocument(null);
        getDocuments();
        setModalOpen(false);
    };

    const handleSave = () => {
        if (document && document.fileId) {
            document.id
                ? dispatch(
                      editDocumentRequest({
                          gridName,
                          cardId,
                          document: {
                              id: document.id,
                              name: document.name,
                              fileId: document.fileId,
                              typeId: document.typeId,
                          },
                          id: document.id,
                          callbackSuccess: () => {
                              handleClose();
                          },
                      }),
                  )
                : dispatch(
                      addDocumentRequest({
                          gridName,
                          cardId,
                          document: {
                              name: document.name,
                              fileId: document.fileId,
                              typeId: document.typeId,
                          },
                          callbackSuccess: () => {
                              handleClose();
                              setDocument(null);
                          },
                      }),
                  );
        }
    };

    const handleDownload = () => {
        dispatch(downloadDocumentRequest({ id: document.fileId }));
    };

    const handleDelete = document => {
        setConfirmation({
            open: true,
            content: t('Delete document', { name: document.name }),
            cancel: () => {
                setConfirmation({ open: false });
            },
            confirm: () => {
                dispatch(
                    deleteDocumentRequest({
                        gridName,
                        cardId,
                        id: document.id,
                        callbackSuccess: () => {
                            getDocuments();
                            load && load();
                            setConfirmation({ open: false });
                        },
                    }),
                );
            },
        });
    };

    const addFile = (id, name) => {
        if (id) {
            setDocument({
                ...document,
                fileId: id,
                name: name,
            });
        }
    };

    const handleTypeChange = (e, { value }) => {
        setDocument({
            ...document,
            typeId: value,
        });
    };

    const getFileName = () => {
        if (!document || !document.name) return '';
        const { name } = document;
        return name.lastIndexOf('.') !== -1 ? name.substr(0, name.lastIndexOf('.')) : '';
    };

    const saveFileName = (e, { value }) => {
        if (!value || !document || !document.name || document.name.lastIndexOf('.') === -1) return;
        const currentName = document.name;
        const extension = currentName.substr(currentName.lastIndexOf('.'));

        setDocument({
            ...document,
            name: value + extension,
        });
    };

    const handleAddition = (e, { value }) => {
        dispatch(
            editCardRequest({
                name: 'documentTypes',
                params: {
                    name: value,
                },
                callbackSuccess: () => dispatch(getDocumentTypesRequest()),
            }),
        );
    };

    return (
        <DocView document={currentDocument}>
            {currentDocument ? (
                <div>
                    {isEditPermissions ? (
                        <Popup
                            content={t('delete')}
                            position="bottom right"
                            trigger={
                                <Icon
                                    name="times"
                                    className="uploaded-image-delete-button"
                                    onClick={() => handleDelete(currentDocument)}
                                />
                            }
                        />
                    ) : null}

                    {isEditPermissions ? (
                        <Popup
                            content={t('edit')}
                            position="bottom right"
                            trigger={
                                <Icon
                                    name="pencil alternate"
                                    className="uploaded-image-edit-button"
                                    onClick={handleOpen}
                                />
                            }
                        />
                    ) : null}

                    <Popup
                        content={t('download')}
                        position="bottom right"
                        trigger={
                            <Icon
                                name="download"
                                className="uploaded-image-download-button"
                                onClick={handleDownload}
                            />
                        }
                    />
                </div>
            ) : (
                <Icon name="plus" className="uploaded-image-add-button" onClick={handleOpen} />
            )}
            <Modal
                size="mini"
                open={modalOpen}
                closeOnEscape
                closeOnDimmerClick={false}
                onClose={handleClose}
            >
                <Modal.Header>{titleText}</Modal.Header>
                <Modal.Content>
                    <Form>
                        <FileUploader document={document} onChange={addFile} />
                        <WebCamUploader onChange={addFile}>
                            <Button size="small" floated="right" icon="photo" content="Webcam" />
                        </WebCamUploader>
                        <Form.Field>
                            <Form.Input
                                label={t('Document name')}
                                placeholder={t('Upload file')}
                                name="name"
                                value={getFileName()}
                                onChange={saveFileName}
                            />
                        </Form.Field>
                        <Form.Field>
                            <Form.Dropdown
                                label={t('Type')}
                                fluid
                                selection
                                name="documentType"
                                allowAdditions
                                search
                                value={document ? document.typeId : null}
                                options={documentTypes}
                                additionLabel={
                                    <i style={{ color: 'blue' }}>{`${t('add value')}: `} </i>
                                }
                                onAddItem={handleAddition}
                                onChange={handleTypeChange}
                            />
                        </Form.Field>
                    </Form>
                </Modal.Content>
                <Modal.Actions>
                    <Button icon="ban" content={t('CancelButton')} onClick={handleClose} />
                    <Button icon="check" positive content={okButtonText} onClick={handleSave} />
                </Modal.Actions>
            </Modal>
            <Confirm
                open={confirmation.open}
                content={confirmation.content}
                onCancel={confirmation.cancel}
                cancelButton={t('cancelConfirm')}
                onConfirm={confirmation.confirm}
            />
        </DocView>
    );
};

export default DocWithEditor;
