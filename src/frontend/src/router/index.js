import React, { useEffect } from 'react';
import { Switch, Redirect, Route } from 'react-router-dom';
import { withRouter } from 'react-router';
import { useSelector } from 'react-redux';
import {
    DICTIONARY_CARD_LINK,
    DICTIONARY_LIST_LINK,
    DICTIONARY_NEW_LINK,
    FIELDS_SETTING_LINK, GRID_AUTO_GROUPING_LINK,
    GRID_CARD_LINK,
    GRID_LIST_LINK,
    GRID_NEW_LINK,
    LOGIN_LINK,
    NEW_ROLE_LINK,
    NEW_USER_LINK, PROFILE_LINK, REPORT_LINK,
    ROLE_LINK,
    ROLES_LINK,
    USER_LINK,
    USERS_LINK,
} from './links';
import CustomGrid from '../containers/customGrid/list';
import CustomGridCard from '../containers/customGrid/card';
import CustomDictionaryList from '../containers/customDictionary/list';
import CustomDictionaryCard from '../containers/customDictionary/card';
import PrivateRoute from './privateRoute';
import Login from '../containers/login';
import RolesList from '../containers/roles/roles_list';
import RoleCard from '../containers/roles/role_card_new';
import UsersList from '../containers/users/users_list';
import UserCard from '../containers/users/user_card_new';
import FieldsSetting from '../containers/fieldsSetting/list';
import { homePageSelector } from '../ducks/profile';
import Profile from "../containers/users/profile";
import AutoGrouping from "../containers/customGrid/autogrouping";
import OperationalReport from "../containers/reports/operationalReport";
import {OPERATIONAL_REPORT_TYPE, REGISTRY_REPORT_TYPE} from "../constants/reportType";
import RegistryReport from "../containers/reports/registryReport";

const MainRoute = withRouter(props => {
    const homePage = useSelector(state => homePageSelector(state));

    useEffect(
        () => {
            const { history, location } = props;
            const { pathname } = location;
            if (pathname === '/grid' && homePage) {
                history.push(homePage);
            }
        },
        [homePage],
    );

    return (
        <Switch>
            <PrivateRoute exact path="/" component={() => <Redirect to={homePage} />} />
            <PrivateRoute exact path={GRID_NEW_LINK} component={CustomGridCard} />
            <PrivateRoute exact path={GRID_AUTO_GROUPING_LINK} permission="autogroupingOrders" component={AutoGrouping} />
            <PrivateRoute exact path={REPORT_LINK.replace(':type', OPERATIONAL_REPORT_TYPE)} permission={OPERATIONAL_REPORT_TYPE} component={OperationalReport} />
            <PrivateRoute exact path={REPORT_LINK.replace(':type', REGISTRY_REPORT_TYPE)} permission={REGISTRY_REPORT_TYPE} component={RegistryReport} />
            <PrivateRoute exact path={GRID_CARD_LINK} component={props => CustomGridCard(props)} />
            <PrivateRoute exact path={GRID_LIST_LINK} component={CustomGrid} />
            <PrivateRoute exact path={DICTIONARY_NEW_LINK} component={CustomDictionaryCard} />
            <PrivateRoute exact path={DICTIONARY_CARD_LINK} component={CustomDictionaryCard} />
            <PrivateRoute exact path={DICTIONARY_LIST_LINK} component={CustomDictionaryList} />
            <PrivateRoute exact path={NEW_ROLE_LINK} permission="editRoles" component={RoleCard} />
            <PrivateRoute exact path={ROLE_LINK} permission="editRoles" component={RoleCard} />
            <PrivateRoute exact path={ROLES_LINK} permission="editRoles" component={RolesList} />
            <PrivateRoute exact path={NEW_USER_LINK} permission="editUsers" component={UserCard} />
            <PrivateRoute exact path={USER_LINK} permission="editUsers" component={UserCard} />
            <PrivateRoute exact path={USERS_LINK} permission="editUsers" component={UsersList} />
            <PrivateRoute exact path={PROFILE_LINK} permission="profile" component={Profile} />
            <PrivateRoute
                exact
                path={FIELDS_SETTING_LINK}
                permission="editFieldProperties"
                component={FieldsSetting}
            />
            <Route exact path={LOGIN_LINK} component={Login} />
            <PrivateRoute exact path="*" component={() => <Redirect to={homePage} />} />
        </Switch>
    );
});

export default MainRoute;
