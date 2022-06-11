import React from 'react';
import { useSelector } from 'react-redux';
import { Redirect, Route } from 'react-router-dom';
import { isAuthSelector } from '../ducks/login';
import {
    dictionariesSelector,
    gridsMenuSelector,
    homePageSelector,
    isCustomPageSelector,
} from '../ducks/profile';
import { customPage } from './links';

function PrivateRoute({ component: Component, ...rest }) {
    const isAuth = useSelector(state => isAuthSelector(state));
    const homePage = useSelector(state => homePageSelector(state));

    const gridsMenu = useSelector(state => gridsMenuSelector(state)) || [];
    const dictionaryMenu = useSelector(state => dictionariesSelector(state)) || [];
    const isCustomPage = useSelector(state => isCustomPageSelector(state)) || {};

    const { computedMatch, location, permission = '' } = rest;
    const { path, params } = computedMatch;

    if (
        (path.includes('grid') && !gridsMenu.includes(params.name)) ||
        (path.includes('dictionary') && !dictionaryMenu.includes(params.name)) ||
        (customPage.includes(path) && !isCustomPage[permission])
    ) {
        return (
            <Redirect
                to={{
                    pathname: homePage,
                    state: { from: location },
                }}
            />
        );
    }

    return (
        <Route
            {...rest}
            render={props =>
                isAuth ? (
                    <Component {...props} />
                ) : (
                    <Redirect
                        to={{
                            pathname: '/login',
                            state: { from: props.location },
                        }}
                    />
                )
            }
        />
    );
}

export default PrivateRoute;
