import React, { Component } from 'react';
import * as ReactDOM from 'react-dom';
import { toast, ToastContainer } from 'react-toastify';

const modalRoot = document.getElementById('toast-root');

class ToastPortalContainer extends Component {
    constructor(props) {
        super(props);
        this.el = document.createElement('div');
    }

    componentDidMount() {
        modalRoot.appendChild(this.el);
    }

    componentWillUnmount() {
        modalRoot.removeChild(this.el);
    }

    render() {
        return ReactDOM.createPortal(
            <ToastContainer
                draggable={false}
                position={toast.POSITION.TOP_RIGHT}
                autoClose={4000}
                hideProgressBar={true}
            />,
            this.el,
        );
    }
}

export default ToastPortalContainer;
