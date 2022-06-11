import React, { useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch } from 'react-redux';
import { Button, Icon, Modal } from 'semantic-ui-react';
import Webcam from 'react-webcam';
import { uploadFileRequest } from '../../ducks/documents';

const DocWithEditor = ({ children, onChange }) => {
    let [modalOpen, setModalOpen] = useState(false);
    let [mode, setMode] = useState('webcam');
    let [imageSrc, setImageSrc] = useState('');
    const { t } = useTranslation();
    const dispatch = useDispatch();

    const webcam = useRef(null);

    const handleOpen = () => {
        setModalOpen(true);
    };

    const handleClose = () => {
        setModalOpen(false);
    };

    const takePhoto = () => {
        const imageSrc = webcam && webcam.current.getScreenshot();
        setMode('image');
        setImageSrc(imageSrc);
    };

    const rephoto = () => {
        setMode('webcam');
        setImageSrc('');
    };

    const save = () => {
        if (imageSrc) {
            dispatch(
                uploadFileRequest({
                    form: {
                        name: 'WebCamPhoto.jpg',
                        body: imageSrc.split(',')[1],
                    },
                    fileName: 'WebCamPhoto.jpg',
                    isBase64: true,
                    callbackSuccess: id => {
                        onChange(id, 'WebCamPhoto.jpg');
                        handleClose();
                    },
                }),
            );
        }
        // callApi("files/uploadBase64File", {
        //     base64: this.state.imageSrc,
        //     fileName: "WebCamPhoto.jpg"
        // })
        //     .then((fileDto) => {
        //         this.props.onChange(fileDto);
        //         this.setState({
        //             modalOpen: false,
        //             imageSrc: ""
        //         });
        //     });
    };
    return (
        <Modal
            trigger={children}
            onOpen={handleOpen}
            open={modalOpen}
            closeOnEscape
            closeOnDimmerClick={false}
            onClose={handleClose}
        >
            <Modal.Header>{t('Create a Photo')}</Modal.Header>
            <Modal.Content>
                {mode === 'image' ? <img src={imageSrc} /> : <Webcam ref={webcam} />}
            </Modal.Content>
            <Modal.Actions>
                <Button color="red" onClick={handleClose}>
                    <Icon name="ban" />
                    {t('CancelButton')}
                </Button>
                {mode === 'webcam' ? (
                    <Button color="green" onClick={takePhoto}>
                        <Icon name="photo" />
                        {t('PhotoButton')}
                    </Button>
                ) : (
                    ''
                )}
                {mode === 'image' ? (
                    <Button color="orange" onClick={rephoto}>
                        <Icon name="photo" /> Rephoto
                    </Button>
                ) : (
                    ''
                )}
                {mode === 'image' ? (
                    <Button color="green" onClick={save}>
                        <Icon name="save" /> Take it
                    </Button>
                ) : (
                    ''
                )}
            </Modal.Actions>
        </Modal>
    );
};

export default DocWithEditor;
