import {
    all,
    put,
    takeEvery,
    take,
    spawn,
    delay,
    cancelled,
    cancel,
    fork,
    select,
} from 'redux-saga/effects';
import { createSelector } from 'reselect';
import { postman } from '../utils/postman';
import { push as historyPush } from 'connected-react-router';
import { FIELDS_SETTING_LINK, ROLES_LINK, USERS_LINK } from '../router/links';
import { logoutRequest } from './login';
import { clearDictionaryInfo } from './dictionaryView';
import result from '../components/SuperGrid/components/result';
import { toast } from 'react-toastify';
import { errorMapping } from '../utils/errorMapping';
import { openSelector } from './representations';
import { OPERATIONAL_REPORT_TYPE, REGISTRY_REPORT_TYPE } from '../constants/reportType';

const TYPE_API = 'profile';

//*  TYPES  *//
export const GET_USER_PROFILE_REQUEST = 'GET_USER_PROFILE_REQUEST';
const GET_USER_PROFILE_SUCCESS = 'GET_USER_PROFILE_SUCCESS';
const GET_USER_PROFILE_ERROR = 'GET_USER_PROFILE_ERROR';

const GET_PROFILE_SETTINGS_REQUEST = 'GET_PROFILE_SETTINGS_REQUEST';
const GET_PROFILE_SETTINGS_SUCCESS = 'GET_PROFILE_SETTINGS_SUCCESS';
const GET_PROFILE_SETTINGS_ERROR = 'GET_PROFILE_SETTINGS_ERROR';

const EDIT_PROFILE_SETTINGS_REQUEST = 'EDIT_PROFILE_SETTINGS_REQUEST';
const EDIT_PROFILE_SETTINGS_SUCCESS = 'EDIT_PROFILE_SETTINGS_SUCCESS';
const EDIT_PROFILE_SETTINGS_ERROR = 'EDIT_PROFILE_SETTINGS_ERROR';

const CHANGE_PASSWORD_REQUEST = 'CHANGE_PASSWORD_REQUEST';
const CHANGE_PASSWORD_SUCCESS = 'CHANGE_PASSWORD_SUCCESS';
const CHANGE_PASSWORD_ERROR = 'CHANGE_PASSWORD_ERROR';

const GET_NOTIFICATIONS_LIST_REQUEST = 'GET_NOTIFICATIONS_LIST_REQUEST';
const GET_NOTIFICATIONS_LIST_SUCCESS = 'GET_NOTIFICATIONS_LIST_SUCCESS';
const GET_NOTIFICATIONS_LIST_ERROR = 'GET_NOTIFICATIONS_LIST_ERROR';

const START_AUTO_UPDATE = 'START_AUTO_UPDATE';
const STOP_AUTO_UPDATE = 'STOP_AUTO_UPDATE';

const CLEAR_ERROR = 'CLEAR_ERROR';
const ADD_ERROR = 'ADD_ERROR';

const DEFAULT_STATE = 'DEFAULT_STATE';

let taskUpdateConfig = null;

//*  INITIAL STATE  *//

const initial = {
    progress: false,
    data: {},
    notifications: [],
    error: [],
    progressEdit: false,
    progressChangePassword: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_USER_PROFILE_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case GET_USER_PROFILE_SUCCESS:
            return {
                ...state,
                progress: false,
                ...payload,
            };
        case GET_USER_PROFILE_ERROR:
            return {
                ...state,
                progress: false,
            };
        case GET_PROFILE_SETTINGS_SUCCESS:
            return {
                ...state,
                data: payload,
            };
        case CLEAR_ERROR:
            return {
                ...state,
                error: state.error && state.error.filter(item => item.name !== payload),
            };
        case ADD_ERROR:
            return {
                ...state,
                error: [...state.error, payload],
            };
        case EDIT_PROFILE_SETTINGS_REQUEST:
            return {
                ...state,
                progressEdit: true,
            };
        case EDIT_PROFILE_SETTINGS_SUCCESS:
            return {
                ...state,
                progressEdit: false,
                error: [],
            };
        case EDIT_PROFILE_SETTINGS_ERROR:
            return {
                ...state,
                progressEdit: false,
                error: payload,
            };
        case CHANGE_PASSWORD_REQUEST:
            return {
                ...state,
                progressChangePassword: true,
            };
        case CHANGE_PASSWORD_SUCCESS:
            return {
                ...state,
                error: [],
                progressChangePassword: false,
            };
        case CHANGE_PASSWORD_ERROR:
            return {
                ...state,
                error: payload,
                progressChangePassword: false,
            };
        case GET_NOTIFICATIONS_LIST_SUCCESS:
            return {
                ...state,
                notifications: payload,
            };
        case DEFAULT_STATE:
            return {
                ...initial
            };
        default:
            return state;
    }
};

//*  ACTION CREATORS  *//

export const getUserProfile = payload => {
    return {
        type: GET_USER_PROFILE_REQUEST,
        payload,
    };
};

export const getProfileSettingsRequest = payload => {
    return {
        type: GET_PROFILE_SETTINGS_REQUEST,
        payload,
    };
};

export const editProfileSettingsRequest = payload => {
    return {
        type: EDIT_PROFILE_SETTINGS_REQUEST,
        payload,
    };
};

export const changePasswordRequest = payload => {
    return {
        type: CHANGE_PASSWORD_REQUEST,
        payload,
    };
};

export const clearError = payload => {
    return {
        type: CLEAR_ERROR,
        payload,
    };
};

export const addError = payload => {
    return {
        type: ADD_ERROR,
        payload,
    };
};

export const getNotificationsListRequest = () => {
    return {
        type: GET_NOTIFICATIONS_LIST_REQUEST,
    };
};

export const startAutoUpdateConfigRequest = () => {
    return {
        type: START_AUTO_UPDATE,
    };
};

export const stopAutoUpdateConfigRequest = () => {
    return {
        type: STOP_AUTO_UPDATE,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.profile;
export const gridsMenuSelector = createSelector(
    stateSelector,
    state => state.grids && state.grids.map(grid => grid.name),
);
export const dictionariesMenuSelector = createSelector(
    stateSelector,
    state =>
        state.dictionaries &&
        state.dictionaries
            .filter(dictionary => !dictionary.showOnHeader)
            .map(dictionary => dictionary.name),
);

export const dictionariesHeaderSelector = createSelector(
    stateSelector,
    state =>
        state.dictionaries &&
        state.dictionaries
            .filter(dictionary => dictionary.showOnHeader)
            .map(dictionary => dictionary.name),
);

export const dictionariesSelector = createSelector(
    stateSelector,
    state => state.dictionaries && state.dictionaries.map(dictionary => dictionary.name),
);

export const isCustomPageSelector = createSelector(
    stateSelector,
    state => {
        return {
            editFieldProperties: state.editFieldProperties,
            editRoles: state.editRoles,
            editUsers: state.editUsers,
            importShippingVehicle: state.importShippingVehicle,
            profile: true,
            autogroupingOrders: state.autogroupingOrders,
            [OPERATIONAL_REPORT_TYPE]: state.viewOperationalReport,
            [REGISTRY_REPORT_TYPE]: state.viewRegistryReport,
        };
    },
);

export const otherMenuSelector = createSelector(
    stateSelector,
    state => {
        const menu = [];
        if (state.editFieldProperties) {
            menu.push({
                name: 'fields_setting',
                link: FIELDS_SETTING_LINK,
            });
        }

        return menu;
    },
);

export const userNameSelector = createSelector(
    stateSelector,
    state => state.userName,
);
export const roleSelector = createSelector(
    stateSelector,
    state => state.userRole,
);
export const roleIdSelector = createSelector(
    stateSelector,
    state => state.role && state.role.id,
);

export const rolesAndUsersMenu = createSelector(
    stateSelector,
    state => {
        let menu = [];

        if (state.editRoles) {
            menu.push({
                name: 'roles',
                link: ROLES_LINK,
            });
        }

        if (state.editUsers) {
            menu.push({
                name: 'users',
                link: USERS_LINK,
            });
        }

        return menu;
    },
);

export const dataLoadingMenuSelector = createSelector(
    stateSelector,
    state => {
        let menu = [];

        if (state.importShippingVehicle) {
            menu.push({
                name: 'drivers_and_vehicles',
                typeApi: 'shippingVehicle',
                items: [
                    {
                        name: 'Template upload',
                        type: 'unloading',
                    },
                    {
                        name: 'data_loading',
                        type: 'loading',
                    },
                ],
            });
        }

        if (state.importOrders) {
            menu.push({
                name: 'orders',
                typeApi: 'orders',
                items: [
                    {
                        name: 'Template upload',
                        type: 'unloading',
                    },
                    {
                        name: 'data_loading',
                        type: 'loading',
                    },
                ],
            });
        }

        if (state.poolingWarehousesImport) {
            menu.push({
                name: 'poolingWarehousesImport',
                typeApi: 'poolingWarehouses',
                items: [
                    {
                        name: 'Template upload',
                        type: 'unloading',
                    },
                    {
                        name: 'data_loading',
                        type: 'loading',
                    },
                ],
            })
        }

    if (state.invoiceImport) {
        menu.push( {
            name: 'invoices',
            typeApi: 'invoices',
            items: [
                {
                    name: 'instruction',
                    type: 'instruction',
                    fileName: 'invoice_howto.pdf',
                },
                {
                    name: 'Template upload',
                    type: 'unloading',
                },
                {
                    name: 'data_loading',
                    type: 'loading'
                },
            ],
        })
    }

        return menu;
    },
);

export const reportsMenuSelector = createSelector(
    stateSelector,
    state => {
        let menu = [];

        if (state.viewOperationalReport) {
            menu.push({
                name: 'Operational report',
                type: OPERATIONAL_REPORT_TYPE,
            });
        }

        if (state.viewRegistryReport) {
            menu.push({
                name: 'Registry report',
                type: REGISTRY_REPORT_TYPE,
            });
        }
        return menu;
    },
);

export const homePageSelector = createSelector(
    stateSelector,
    state => {
        let homePage = '/grid';
        if (state.grids && state.grids.length) {
            homePage = `/grid/${state.grids[0].name}`;
        } else if (state.dictionaries && state.dictionaries.length) {
            homePage = `/dictionary/${state.dictionaries[0].name}`;
        } else if (state.editRoles) {
            homePage = '/roles';
        } else if (state.editUsers) {
            homePage = '/users';
        }

        return homePage;
    },
);

export const userPermissionsSelector = createSelector(
    stateSelector,
    state => {
        return state.role ? state.role.permissions.map(item => item.code) : [];
    },
);

export const profileSettingsSelector = createSelector(
    stateSelector,
    state => state.data,
);

export const progressEditSelector = createSelector(
    stateSelector,
    state => state.progressEdit,
);

export const progressChangePasswordSelector = createSelector(
    stateSelector,
    state => state.progressChangePassword,
);

export const errorSelector = createSelector(
    stateSelector,
    state => errorMapping(state.error),
);

export const notificationsSelector = createSelector(
    stateSelector,
    state => state.notifications,
);

//*  SAGA  *//

export function* getUserProfileSaga({ payload = {} }) {
    try {
        const { url, isNotCofigUpdate } = payload;
        const userInfo = yield postman.get('/identity/userInfo');
        const config = isNotCofigUpdate ? {} : yield postman.get('/appConfiguration');

        yield put({
            type: GET_USER_PROFILE_SUCCESS,
            payload: { ...userInfo, ...config },
        });
        if (url) {
            yield put(historyPush(url));
        }
    } catch (e) {
        yield put({
            type: GET_USER_PROFILE_ERROR,
            payload: e,
        });

        yield put(logoutRequest()); // todo
    }
}

function* getProfileSettingsSaga() {
    try {
        const result = yield postman.get(`/${TYPE_API}/info`);

        yield put({
            type: GET_PROFILE_SETTINGS_SUCCESS,
            payload: result,
        });
    } catch (e) {
        yield put({
            type: GET_PROFILE_SETTINGS_ERROR,
        });
    }
}

function* editProfileSettingsSaga({ payload }) {
    try {
        const { form, callbackSuccess } = payload;
        const result = yield postman.post(`/${TYPE_API}/save`, form);

        if (result.isError) {
            toast.error(result.message);
            yield put({
                type: EDIT_PROFILE_SETTINGS_ERROR,
                payload: result.errors,
            });
        } else {
            yield put({
                type: EDIT_PROFILE_SETTINGS_SUCCESS,
                payload: result,
            });

            yield put({
                type: GET_USER_PROFILE_SUCCESS,
                payload: { userName: form.userName },
            });

            callbackSuccess && callbackSuccess();
        }
    } catch (e) {
        yield put({
            type: EDIT_PROFILE_SETTINGS_ERROR,
        });
    }
}

function* changePasswordSaga({ payload }) {
    try {
        const { form, callbackSuccess, t } = payload;
        const result = yield postman.post(`/${TYPE_API}/setNewPassword`, form);

        if (result.isError) {
            toast.error(result.message);

            yield put({
                type: CHANGE_PASSWORD_ERROR,
                payload: result.errors,
            });
        } else {
            toast.info(t('changePasswordSuccess'));

            yield put({
                type: CHANGE_PASSWORD_SUCCESS,
                payload: result,
            });

            callbackSuccess && callbackSuccess();
        }
    } catch (e) {
        yield put({
            type: CHANGE_PASSWORD_ERROR,
        });
    }
}

function* getNotificationsListSaga({ payload }) {
    try {
        const result = yield postman.get(`/${TYPE_API}/allNotifications`);

        yield put({
            type: GET_NOTIFICATIONS_LIST_SUCCESS,
            payload: result,
        });
    } catch (e) {
        yield put({
            type: GET_NOTIFICATIONS_LIST_ERROR,
        });
    }
}

const startAutoUpdateConfigSaga = function*() {
    const isOpenRepresentation = yield select(openSelector);

    if (!isOpenRepresentation) {
        taskUpdateConfig = yield fork(backgroundSyncListSaga);
    }
};

const stopAutoUpdateConfigSaga = function*() {
    if (taskUpdateConfig) {
        yield cancel(taskUpdateConfig);
        taskUpdateConfig = null;
    }
};

export const backgroundSyncListSaga = function*() {
    try {
        while (true) {
            yield put(getUserProfile());
            yield delay(600000);
        }
    } finally {
        if (yield cancelled()) {
            console.log('---', 'cancelled sync saga!!');
        }
    }
};

export function* saga() {
    yield all([
        takeEvery(GET_USER_PROFILE_REQUEST, getUserProfileSaga),
        takeEvery(GET_PROFILE_SETTINGS_REQUEST, getProfileSettingsSaga),
        takeEvery(EDIT_PROFILE_SETTINGS_REQUEST, editProfileSettingsSaga),
        takeEvery(CHANGE_PASSWORD_REQUEST, changePasswordSaga),
        takeEvery(GET_NOTIFICATIONS_LIST_REQUEST, getNotificationsListSaga),
        takeEvery(START_AUTO_UPDATE, startAutoUpdateConfigSaga),
        takeEvery(STOP_AUTO_UPDATE, stopAutoUpdateConfigSaga),
    ]);
}
