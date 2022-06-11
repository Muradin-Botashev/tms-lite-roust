import React, { useEffect } from 'react';
import { Button, Checkbox, Dimmer, Form, Icon, Loader, Popup } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import {
    clearLookup,
    getLookupRequest,
    listSelector,
    progressSelector,
    stateListSelector,
} from '../../ducks/lookup';

const Facet = ({value, onChange, sort, setSort, name, source, gridName, filters}) => {
    const { t } = useTranslation();

    const dispatch = useDispatch();

    let values = value ? value.split('|') : [];

    const toggle = (e, { value }) => {
        if (values.some(x => x === value)) {
            values.splice(values.indexOf(value), 1);
        } else {
            values.push(value);
        }
        if (onChange !== undefined) onChange(e, { name, value: values.join('|') });
    };

    useEffect(() => {
        handleOpen();
        return handleClose;
    }, []);

    const handleOpen = () => {
        dispatch(
            getLookupRequest({
                name,
                entities: gridName,
                isFilter: true,
                params: filters.filter,
            }),
        );
    };

    const handleClose = () => {
        dispatch(clearLookup());
    };

    const handleRestClick = () => {
        if (onChange !== undefined) onChange(null, { name: name, value: null });
    };

    const stateColors = useSelector(state => stateListSelector(state)) || [];
    const loading = useSelector(state => progressSelector(state));

    return (
        <div className="facet-sortable facet-input">
            <Form style={{ minWidth: '50px', minHeight: '50px' }}>
                <Dimmer active={loading} inverted>
                    <Loader size="small">Loading</Loader>
                </Dimmer>
                <div className="reset-selected">
                    <span onClick={handleRestClick}>{t('reset_selected')}</span>
                </div>
                {stateColors.map(x => {
                    let label = (
                        <label>
                            <Icon
                                color={x.color ? x.color.toLowerCase() : 'grey'}
                                inverted={x.inverted}
                                name="circle"
                            />
                            {t(x.name)}
                        </label>
                    );
                    return (
                        <Form.Field key={x.value}>
                            <Checkbox
                                value={x.value}
                                checked={values.includes(x.value)}
                                onChange={toggle}
                                label={label}
                            />
                        </Form.Field>
                    );
                })}
            </Form>
            {/* <Popup
                trigger={
                    <Button size="small" style={{ lineHeight: '1.1rem' }} fluid>
                        {values.length > 0
                            ? t('selected_count', { count: values.length })
                            : t('All')}
                    </Button>
                }
                content={content}
                on="click"
                className="from-popup"
                hideOnScroll
                position="bottom left"
                onOpen={handleOpen}
                onClose={handleClose}
            />
            <Button
                className={`sort-button sort-button-up ${
                    sort === 'asc' ? 'sort-button-active' : ''
                }`}
                name={name}
                value="asc"
                onClick={setSort}
            >
                <Icon name="caret up" />
            </Button>
            <Button
                className={`sort-button sort-button-down ${
                    sort === 'desc' ? 'sort-button-active' : ''
                }`}
                name={name}
                value="desc"
                onClick={setSort}
            >
                <Icon name="caret down" />
            </Button>*/}
        </div>
    );
};
export default Facet;
