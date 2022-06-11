import {ACTIVE_TYPE, ENUM_TYPE, LABELS_TYPE, SELECT_TYPE, TEXT_TYPE} from './columnTypes';

export const rolesColumns = [
    {
        name: 'name',
        displayNameKey: 'name',
        type: TEXT_TYPE
    },
    {
        name: 'usersCount',
        displayNameKey: 'usersCount',
        type: TEXT_TYPE,
        notSortAndFilter: true
    },
    {
        name: 'permissions',
        displayNameKey: 'permissions',
        type: LABELS_TYPE,
        notSortAndFilter: true
    },
    {
        name: 'companyId',
        displayNameKey: 'companyId',
        type: SELECT_TYPE,
        source: 'companies',
    },
    {
        name: 'isActive',
        displayNameKey: 'isActive',
        type: ACTIVE_TYPE,
    },
];
