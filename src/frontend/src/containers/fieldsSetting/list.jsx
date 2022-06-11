import React, { useEffect, useRef, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import { Loader, Table } from 'semantic-ui-react';
import { gridsMenuSelector, roleIdSelector, roleSelector } from '../../ducks/profile';
import { getLookupRequest, valuesListSelector } from '../../ducks/lookup';
import { columnsGridSelector } from '../../ducks/gridList';
import './style.scss';
import InfiniteScrollTable from '../../components/InfiniteScrollTable';
import {
    clearFieldsSettings,
    editFieldsSettingRequest,
    editProgressSelector,
    fieldsSettingSelector,
    getFieldsSettingRequest,
    fieldsSettingCompaniesSelector,
    getFieldsSettingCompaniesRequest,
    progressSelector,
    toggleHidenStateRequest,
} from '../../ducks/fieldsSetting';
import { ORDERS_GRID } from '../../constants/grids';
import Header from './components/header';
import TableBody from './components/table_body';
import TableHeader from './components/table_header';
import { sortFunc } from '../../utils/sort';

const List = () => {
    const { t } = useTranslation();
    const dispatch = useDispatch();
    const containerRef = useRef(null);

    const gridsList = useSelector(state => gridsMenuSelector(state)) || [];
    const rolesList = useSelector(state => valuesListSelector(state, 'roles')) || [];
    const companiesList = useSelector(state => fieldsSettingCompaniesSelector(state)) || [];
    const settings = useSelector(state => fieldsSettingSelector(state)) || [];
    const loading = useSelector(state => progressSelector(state));
    const editProgress = useSelector(state => editProgressSelector(state)) || false;
    const userRole = useSelector(state => roleIdSelector(state));

    let [activeItem, setActiveItem] = useState(gridsList[0] || '');
    let [role, setRole] = useState(null);
    let [company, setCompany] = useState(null);

    const statusList =
        useSelector(state =>
            valuesListSelector(state, `${activeItem.slice(0, activeItem.length - 1)}State`),
        ) || [];

    useEffect(() => {
        dispatch(
            getLookupRequest({
                name: 'roles',
                isForm: true,
            }),
        );

        dispatch(
            getFieldsSettingCompaniesRequest(),
        );
    }, [userRole]);

    useEffect(() => {
        activeItem && getStatus();
    }, [activeItem]);

    useEffect(() => {
        dispatch(clearFieldsSettings());
        activeItem && role && company && getSettings();
    }, [role, company, activeItem]);

    useEffect(() => {
        if (!role && rolesList.length) {
            setRole(userRole);
        }
    }, [rolesList]);

    useEffect(() => {
        if (!company && companiesList.length) {
            setCompany(companiesList[0].value);
        }
    }, [companiesList]);

    const getSettings = () => {
        dispatch(
            getFieldsSettingRequest({
                forEntity: activeItem,
                roleId: role,
                companyId: company,
            }),
        );
    };

    const getStatus = () => {
        dispatch(
            getLookupRequest({
                name: `${activeItem.slice(0, activeItem.length - 1)}State`,
                isForm: true,
                isState: true,
                params: {},
            }),
        );
    };

    const handleChangeActiveItem = useCallback((e, { name }) => {
        setActiveItem(name);
    }, []);

    const handleChangSettings = useCallback(
        (fieldName, accessType, state = null, isExt) => {
            dispatch(
                editFieldsSettingRequest({
                    params: {
                        forEntity: activeItem,
                        roleId: role === 'null' ? undefined : role,
                        companyId: company === 'null' ? undefined : company,
                        fieldName,
                        accessType,
                        state,
                    },
                    isExt,
                    callbackSuccess: () => {
                        getSettings();
                    },
                }),
            );
        },
        [activeItem, role, company],
    );

    const handleToggleHidden = useCallback(
        (fieldName, isExt) => {
            dispatch(
                toggleHidenStateRequest({
                    params: {
                        forEntity: activeItem,
                        fieldName,
                        roleId: role === 'null' ? undefined : role,
                        companyId: company === 'null' ? undefined : company,
                    },
                    isExt,
                    callbackSuccess: getSettings,
                }),
            );
        },
        [activeItem, role, company],
    );

    const handleChangeRole = useCallback((e, { value }) => {
        setRole(value);
    }, []);

    const handleChangeCompany = useCallback((e, { value }) => {
        setCompany(value);
    }, []);

    const { base: baseSettings = [], ext: extSettings = [] } = settings;

    return (
        <div className="container">
            <Loader active={loading && !baseSettings.length} size="huge" className="table-loader">
                Loading
            </Loader>
            <Header
                gridsList={gridsList}
                activeItem={activeItem}
                changeActiveItem={handleChangeActiveItem}
                rolesList={rolesList}
                companiesList={companiesList}
                role={role}
                company={company}
                changeRole={handleChangeRole}
                changeCompany={handleChangeCompany}
                t={t}
            />
            <div className={`scroll-table-container field-settings-table`} ref={containerRef}>
                <InfiniteScrollTable
                    className="grid-table table-info"
                    onBottomVisible={() => {}}
                    structured
                    context={containerRef.current}
                    headerRow={<TableHeader statusList={statusList} t={t} />}
                >
                    <Table.Body className="table-fields-setting">
                        {baseSettings.length ? (
                            <>
                                <Table.Row>
                                    <Table.Cell>
                                        <div className="ui ribbon label">{t('Main fields')}</div>
                                    </Table.Cell>
                                    {statusList.map((state, i) => (
                                        <Table.Cell key={i} />
                                    ))}
                                </Table.Row>
                                {sortFunc(baseSettings, t, 'fieldName').map(column => (
                                    <TableBody
                                        key={column.fieldName}
                                        column={column}
                                        statusList={statusList}
                                        editProgress={editProgress}
                                        changeSettings={handleChangSettings}
                                        toggleHidden={handleToggleHidden}
                                        t={t}
                                    />
                                ))}
                            </>
                        ) : null}

                        {extSettings.length ? (
                            <>
                                <Table.Row>
                                    <Table.Cell>
                                        <div className="ui ribbon label">
                                            {activeItem === ORDERS_GRID
                                                ? t('articles')
                                                : t('route')}
                                        </div>
                                    </Table.Cell>
                                    {statusList.map((state, i) => (
                                        <Table.Cell key={i} />
                                    ))}
                                </Table.Row>
                                {sortFunc(extSettings, t, 'fieldName').map(column => (
                                    <TableBody
                                        key={column.fieldName}
                                        column={column}
                                        statusList={statusList}
                                        changeSettings={handleChangSettings}
                                        editProgress={editProgress}
                                        toggleHidden={handleToggleHidden}
                                        t={t}
                                        isExt
                                    />
                                ))}
                            </>
                        ) : null}
                    </Table.Body>
                </InfiniteScrollTable>
            </div>
        </div>
    );
};

export default List;
