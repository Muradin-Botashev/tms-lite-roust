import React, { useRef, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Button, Icon, Message, Modal } from 'semantic-ui-react';
import './style.scss';
import InfoComponent from './InfoComponent';
import { contentSelector, hideModal, isOpenSelector } from '../../ducks/modal';
import * as ReactDOM from 'react-dom';

const modalRoot = document.getElementById('modal-root');
let el = document.createElement('div');

const ModalInfo = () => {
    const dispatch = useDispatch();

    const open = useSelector(state => isOpenSelector(state));
    const content = useSelector(state => contentSelector(state));
    const { message, entries, component } = content;

    const onClose = () => {
        dispatch(hideModal());
    };

    return (
        <>
            {component ? (
                <Modal open={open} {...component.props}>
                    <Modal.Header>{component.header}</Modal.Header>
                    <Modal.Content>{component.content}</Modal.Content>
                    <Modal.Actions>
                        {component.footer}
                    </Modal.Actions>
                </Modal>
            ) : (
                <Modal
                    open={open}
                    closeIcon
                    closeOnDimmerClick={false}
                    className="modal-info"
                    onClose={onClose}
                >
                    <Modal.Header>{message}</Modal.Header>
                    <Modal.Content scrolling>
                        {entries && entries.length
                            ? entries.map(item => <InfoComponent item={item} />)
                            : null}
                    </Modal.Content>
                    <Modal.Actions>
                        <Button primary onClick={onClose}>
                            OK
                        </Button>
                    </Modal.Actions>
                </Modal>
            )}
        </>
    );
};

export default ModalInfo;
