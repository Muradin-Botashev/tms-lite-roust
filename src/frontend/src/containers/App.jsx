import React, { useEffect } from 'react';
import { history } from '../../src/store/configureStore';
import { ConnectedRouter } from 'connected-react-router';
import MainRoute from '../router';
import Header from '../components/Header';
import { Scrollbars } from 'react-custom-scrollbars';

import { useDispatch, useSelector } from 'react-redux';

import ToastPortalContainer from '../components/ToastContainer';
import { getUserProfile, userNameSelector } from '../ducks/profile';
import { isAuthSelector } from '../ducks/login';
import { Dimmer, Loader } from 'semantic-ui-react';
import ModalInfo from "../components/ModalInfo";

const App = () => {
    const dispatch = useDispatch();
    const userName = useSelector(state => userNameSelector(state));
    const isAuth = useSelector(state => isAuthSelector(state));

    const getProfile = () => {
        if (!userName) {
            dispatch(getUserProfile());
        }
    };

    useEffect(getProfile, []);

    console.log(userName, isAuth);

    return (
        <>
            <ConnectedRouter history={history}>
                {userName || !isAuth ? (
                    <>
                        <Header />
                        <MainRoute />
                    </>
                ) : (
                    <Dimmer active inverted>
                        <Loader size="huge">Loading</Loader>
                    </Dimmer>
                )}
            </ConnectedRouter>
            <ToastPortalContainer />
            <ModalInfo />
        </>
    );
};

export default App;
