import { all, put, takeEvery, select } from 'redux-saga/effects';
import { createSelector } from 'reselect';
import { postman } from '../utils/postman';
import { representationFromGridSelector } from './representations';

//*  TYPES  *//

const GET_LOOKUP_REQUEST = 'GET_LOOKUP_REQUEST';
const GET_LOOKUP_SUCCESS = 'GET_LOOKUP_SUCCESS';
const GET_EDIT_LOOKUP_SUCCESS = 'GET_EDIT_LOOKUP_SUCCESS';
const GET_LOOKUP_ERROR = 'GET_LOOKUP_ERROR';

const CLEAR_LOOKUP = 'CLEAR_LOOKUP';

const CLEAR_FORM_LOOKUP = 'CLEAR_FORM_LOOKUP';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    list: [],
    progress: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_LOOKUP_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case GET_LOOKUP_SUCCESS:
            return {
                ...state,
                list: payload,
                progress: false,
            };
        case GET_EDIT_LOOKUP_SUCCESS:
            return {
                ...state,
                [payload.key]: payload.list,
                progress: false,
            };
        case GET_LOOKUP_ERROR:
            return {
                ...state,
                progress: false,
            };
        case CLEAR_LOOKUP:
            return {
                ...state,
                list: [],
            };
        case CLEAR_FORM_LOOKUP:
            return {
                ...state,
                [payload]: [],
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

export const getLookupRequest = payload => {
    return {
        type: GET_LOOKUP_REQUEST,
        payload,
    };
};

export const clearLookup = payload => {
    return {
        type: CLEAR_LOOKUP,
        payload,
    };
};

export const clearFormLookup = payload => {
    return {
        type: CLEAR_FORM_LOOKUP,
        payload,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.lookup;
export const listSelector = createSelector(
    [stateSelector, (state, filter) => filter, (state, filter, t) => t],
    (state, filter, t) =>
        state.list
            ? state.list
                  .filter(
                      x =>
                          !x.isBulkUpdateOnly &&
                          (filter
                              ? x.name
                                  ? x.name.toLowerCase().includes(filter.toLowerCase())
                                  : false
                              : true),
                  )
                  .map(item => ({
                      value: item.value,
                      name: t(item.name),
                      isActive: item.isActive,
                  }))
            : [],
);

export const stateListSelector = createSelector(
    stateSelector,
    state => state.list,
);

export const progressSelector = createSelector(
    stateSelector,
    state => state.progress,
);

export const valuesListSelector = createSelector(
    [stateSelector, (state, key) => key, (state, key, isBulkUpdateOnly) => isBulkUpdateOnly],
    (state, key, isBulkUpdateOnly) =>
        state[key]
            ? state[key].filter(item =>
                  isBulkUpdateOnly
                      ? !item.isFilterOnly
                      : !item.isFilterOnly && !item.isBulkUpdateOnly,
              )
            : [],
);

export const listFromMultiSelect = createSelector(
    [
        stateSelector,
        (state, key) => key,
        (state, key, isTranslate) => isTranslate,
        (state, key, isTranslate, t) => t,
    ],
    (state, key, isTranslate, t) =>
        state[key] ? state[key].map((item, index) => ({
            ...item,
            value: item.value,
            text: isTranslate ? t(item.name) : item.name,
        })) : [],
);

export const totalCounterSelector = createSelector(
    [
        stateSelector,
        (state, key) => valuesListSelector(state, key),
        (state, key, t) => t,
        (state, key, t, filter) => filter,
        (state, key, t, filter, isTranslate) => isTranslate,
    ],
    (state, list, t, filter, isTranslate) =>
        list
            ? list
                  .map(item => ({
                      ...item,
                      value: item.value,
                      name: isTranslate ? t(item.name) : item.name,
                  }))
                  .filter(x =>
                      filter ? (x.name ? x.name.toLowerCase().includes(filter) : false) : true,
                  ).length
            : 0,
);

export const listFromSelectSelector = createSelector(
    [
        stateSelector,
        (state, key, t, filter, isTranslate, counter, isBulkUpdateOnly) =>
            valuesListSelector(state, key, isBulkUpdateOnly),
        (state, key, t) => t,
        (state, key, t, filter) => filter,
        (state, key, t, filter, isTranslate) => isTranslate,
        (state, key, t, filter, isTranslate, counter) => counter,
    ],
    (state, list, t, filter, isTranslate, counter) => {
        return list
            ? list
                  .map(item => ({
                      ...item,
                      value: item.value,
                      name: isTranslate ? t(item.name) : item.name,
                  }))
                  .filter(x =>
                      filter
                          ? x.name
                              ? x.name.toLowerCase().includes(filter.toLowerCase())
                              : false
                          : true,
                  )
                  .slice(0, counter)
            : [];
    },
);

//*  SAGA  *//

function* getLookupSaga({ payload }) {
    try {
        const {
            name,
            isForm,
            callbackSuccess,
            callbackFunc,
            isState,
            isFilter,
            entities,
            params = {},
            sourceParams = {},
        } = payload;
        let result;

        const representation = yield select(state =>
            representationFromGridSelector(state, entities),
        );
        const columns = representation ? representation.map(item => item.name) : [];

        if (isState) {
            result = yield postman.post(`/${name}/search`);
        } else if (isFilter) {
            result = yield postman.post(`/${entities}/forSelect/${name}`, {
                ...params,
                filter: {
                    ...params.filter,
                    columns,
                },
            });
        } else {
            result = yield postman.post(`/${name}/forSelect`, sourceParams);
        }

        if (isForm) {
            yield put({
                type: GET_EDIT_LOOKUP_SUCCESS,
                payload: {
                    key: name,
                    list: result.items || result,
                },
            });

            callbackSuccess && callbackSuccess(result);
            callbackFunc && callbackFunc();
        } else {
            yield put({
                type: GET_LOOKUP_SUCCESS,
                payload: result,
            });
        }
    } catch (e) {
        yield put({
            type: GET_LOOKUP_ERROR,
            payload: e,
        });
    }
}

export function* saga() {
    yield all([takeEvery(GET_LOOKUP_REQUEST, getLookupSaga)]);
}
