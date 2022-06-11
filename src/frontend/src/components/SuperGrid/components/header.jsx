import React, { useEffect, useRef } from 'react';
import {Button, Grid, Icon, Message, Popup} from 'semantic-ui-react';
import Search from '../../../components/Search';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import {
    autogroupingOrdersSelector,
    canExportToExcelSelector,
    canImportFromExcelSelector,
    exportProgressSelector,
    exportToExcelRequest,
    importFromExcelRequest,
    importProgressSelector,
} from '../../../ducks/gridList';

import CustomIcon from '../../CustomIcon'

import FieldsConfig from './representations';
import {
    getRepresentationsRequest,
    representationsSelector,
    setRepresentationRequest,
} from '../../../ducks/representations';
import { GRID_AUTO_GROUPING_LINK } from '../../../router/links';
import useReactRouter from 'use-react-router';
import {ORDERS_GRID} from "../../../constants/grids";
import {ordersLoadingSelector} from "../../../ducks/loadingData";

const Header = ({
    isCreateBtn,
    searchValue,
    searchOnChange,
    counter,
    clearFilter,
    disabledClearFilter,
    loadList,
    name,
    setSelected,
    representationName,
    filter,
    goToCard,
    width,
    pageLoading,
    selectedRows
}) => {
    const { t } = useTranslation();

    const dispatch = useDispatch();

    const fileUploader = useRef(null);

    const { history } = useReactRouter();

    const isImportBtn = useSelector(state => canImportFromExcelSelector(state, name));
    const isExportBtn = useSelector(state => canExportToExcelSelector(state, name));

    const importLoader = useSelector(state => importProgressSelector(state));
    const exportLoader = useSelector(state => exportProgressSelector(state));
    const loadingOrders = useSelector(ordersLoadingSelector);

    const representations = useSelector(state => representationsSelector(state, name));

    const exportExcel = () => {
        dispatch(exportToExcelRequest({ name, filter: filter.filter }));
    };

    const importExcel = () => {
        fileUploader && fileUploader.current.click();
    };

    const onFilePicked = e => {
        const file = e.target.files[0];

        const data = new FormData();
        data.append('FileName', file.name);
        data.append('FileContent', new Blob([file], { type: file.type }));
        data.append('FileContentType', file.type);

        dispatch(
            importFromExcelRequest({
                name,
                form: data,
                callbackSuccess: () => loadList(false, true),
            }),
        );
    };

    const getRepresentations = callBackFunc => {
        dispatch(getRepresentationsRequest({ key: name, callBackFunc }));
    };

    const changeRepresentation = (key, isEdit) => {
        dispatch(
            setRepresentationRequest({
                gridName: name,
                value: key,
                callbackSuccess: () => {
                    setSelected(new Set());
                    pageLoading && pageLoading();
                },
            }),
        );
    };

    const handleGoToCard = () => {
        goToCard(false, null, name);
    };

    return (
        <Grid className="grid-header-panel">
            <Grid.Row>
                <Grid.Column width={5}>
                    <FieldsConfig
                        gridName={name}
                        width={width}
                        representationName={representationName}
                        getRepresentations={getRepresentations}
                        changeRepresentation={changeRepresentation}
                        representations={representations}
                    />
                </Grid.Column>
                <Grid.Column width={1} verticalAlign="middle">
                    <span className="records-counter">{t('totalCount', { count: counter })}</span>
                </Grid.Column>
                <Grid.Column width={3}>
                    {
                        name === ORDERS_GRID && loadingOrders
                        ? <Message info>
                            <Icon name="clock"/>
                                Идет загрузка данных...
                            </Message>
                        : null
                    }

                </Grid.Column>
                <Grid.Column width={7} className="grid-right-elements">
                    {isCreateBtn && (
                        <Popup
                            content={t('add_record')}
                            position="bottom right"
                            trigger={<Button icon="add" onClick={handleGoToCard} />}
                        />
                    )}
                    {isImportBtn && (
                        <Popup
                            content={t('importFromExcel')}
                            position="bottom right"
                            trigger={
                                <Button
                                    icon="upload"
                                    loading={importLoader}
                                    onClick={importExcel}
                                />
                            }
                        />
                    )}
                    {isExportBtn && (
                        <Popup
                            content={
                                t('exportToExcel') // todo
                            }
                            position="bottom right"
                            trigger={
                                <Button
                                    icon="download"
                                    loading={exportLoader}
                                    onClick={exportExcel}
                                />
                            }
                        />
                    )}
                    <Popup
                        content={t('reset_filters')}
                        position="bottom right"
                        trigger={
                            <Button
                                icon
                                className={`clear-filter-btn`}
                                onClick={clearFilter}
                                disabled={disabledClearFilter}
                            >
                                <CustomIcon name="clear-filter" />
                            </Button>
                        }
                    />
                    <Search
                        searchValue={searchValue}
                        className="search-input"
                        value={filter.filter.filter.search}
                        onChange={searchOnChange}
                    />
                </Grid.Column>
            </Grid.Row>
            <input
                type="file"
                ref={fileUploader}
                style={{ display: 'none' }}
                onInput={onFilePicked}
            />
        </Grid>
    );
};

export default Header;
